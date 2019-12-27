using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using ZipperLib.Common;

namespace ZipperLib.Domain.ZipperService
{
    public partial class ZipperService
    {
        private void CompressFile()
        {
            WriteInputFileInfo();
            using (var pool = new Pool())
            {
                Console.WriteLine("Compressing...");
                var nextIndex = 0L;
                _onProcessingBlockCount = 0;
                var readWaiter = new AutoResetEvent(false);
                var writeWaiter = new AutoResetEvent(false);
                _blocks = new ConcurrentDictionary<long, DataBlock>();
                _inputBlocks = new ConcurrentDictionary<long, InputBlock>();
                var threadCount = pool.ThreadCount;
                var writer = new Thread(() =>
                {
                    StartCompressorWriter(writeWaiter);
                })
                {
                    IsBackground = true
                };
                writer.Start();
                var reader = new Thread(() =>
                {
                    StartCompressorReader(readWaiter, threadCount);
                })
                {
                    IsBackground = true
                };
                reader.Start();


                var writerWork = writeWaiter.WaitOne(0) == false;
                while (writerWork)
                {
                    var isSuccessRemoved = _inputBlocks.TryRemove(nextIndex, out InputBlock inputBlock);
                    if (isSuccessRemoved)
                    {
                        _onProcessingBlockCount++;
                        pool.QueueTask(() =>
                        {
                            var zipper = new Zipper();
                            var block = CompressBlock(zipper, inputBlock);
                            _blocks[inputBlock.Index] = block;
                            _onProcessingBlockCount--;
                        });
                        nextIndex++;
                    }
                    writerWork = writeWaiter.WaitOne(0) == false;
                }

            }
            Console.WriteLine($"Compressed {_config.Input.Name} " +
                              $"from {_config.Input.Length.ToString()} " +
                              $"to {_config.Output.Length.ToString()} bytes.");
        }

        private DataBlock CompressBlock(Zipper zipper, InputBlock inputBlock)
        {
            var compressedData = zipper.Compress(inputBlock.Data);
            var header = new BlockHeader(inputBlock.Index, inputBlock.Length, compressedData.Length);
            var block = new DataBlock(header, compressedData);

            return block;
        }

        private void StartCompressorReader(AutoResetEvent readWaiter, int threadCount)
        {
            using (var reader = _config.Input.OpenRead())
            {
                var index = 0L;
                int readBytes;
                var buffer = new byte[_config.BufferSize];
                while ((readBytes = reader.Read(buffer, 0, _config.BufferSize)) != 0)
                {
                    if (_inputBlocks.Count >= threadCount)
                    {
                        // wait until 50 % threads release 
                        SpinWait.SpinUntil(() => _onProcessingBlockCount < threadCount*0.5);
                    }

                    var bufferTemp = buffer.Clone() as byte[];
                    _inputBlocks[index] = new InputBlock(index, readBytes, bufferTemp);
                    index++;
                }

                readWaiter.Set();
            }
        }

        private void StartCompressorWriter(AutoResetEvent writeWaiter)
        {
            using (var writer = _config.Output.OpenWrite())
            {
                var nextIndex = 0L;
                var writtenLength = 0L;
                var inputFileLength = _config.Input.Length;
                var hwr = new HeaderReaderWriter();
                writer.Position = sizeof(long);
                while (writtenLength < inputFileLength)
                {
                    while (_blocks.ContainsKey(nextIndex))
                    {
                        var block = _blocks[nextIndex];
                        var headerBuffer = hwr.CreateBlockHeader(block.Header);
                        writer.Write(headerBuffer, 0, headerBuffer.Length);
                        writer.Write(block.Data, 0, block.Header.CompressedDataLength);

                        _blocks.TryRemove(nextIndex, out _);
                        writtenLength += block.Header.DecompressedDataLength;
                        nextIndex++;
                    }
                }
            }

            writeWaiter.Set();
        }
        private void WriteInputFileInfo()
        {
            using (FileStream compressedFileStream = File.Create(_config.Output.FullName))
            {
                var inputFileLengthInfo = BitConverter.GetBytes(_config.Input.Length);
                compressedFileStream.Write(inputFileLengthInfo, 0, inputFileLengthInfo.Length);
            }
        }
    }
}
