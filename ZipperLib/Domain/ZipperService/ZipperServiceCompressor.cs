using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using ZipperLib.Common;

namespace ZipperLib.Domain.ZipperService
{
    public class ZipperServiceCompressor
    {
        public ZipperServiceCompressor(ZipperServiceConfig config)
        {
            _config = config;
        }

        public void CompressFile()
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
            using (var reader = _config.Input.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var index = 0L;
                int readBytes;
                var buffer = new byte[_config.BufferSize];
                while ((readBytes = reader.Read(buffer, 0, _config.BufferSize)) > 0)
                {
                    if (_inputBlocks.Count >= threadCount)
                    {
                        SpinWait.SpinUntil(() => _onProcessingBlockCount < threadCount);
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
                var inputFileLength = _config.Input.Length;
                var writtenLength = 0L;

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
                        writtenLength = writtenLength + block.Header.DecompressedDataLength;
                        _blocks.TryRemove(nextIndex, out _);
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

        private static int _onProcessingBlockCount;
        private volatile ZipperServiceConfig _config;
        private ConcurrentDictionary<long, DataBlock> _blocks;
        private ConcurrentDictionary<long, InputBlock> _inputBlocks;
    }
}
