using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using ZipperLib.Common;
using ZipperLib.Exceptions;

namespace ZipperLib.Domain.ZipperService
{
    public partial class ZipperService
    {
        private void DecompressFile()
        {
            var blocksCount = DecompressionPrepare();
            using (var pool = new Pool())
            {
                Console.WriteLine("Decompressing...");
                var nextIndex = 0L;
                _onProcessingBlockCount = 0;
                if (blocksCount <= 0)
                {
                    Console.WriteLine("Error decompress file");
                    return;
                }
                _blocks = new ConcurrentDictionary<long, DataBlock>();
                _inputBlocks = new ConcurrentDictionary<long, InputBlock>();
                var writeWaiter = new AutoResetEvent(false);
                var readWaiter = new AutoResetEvent(false);
                var threadCount = pool.ThreadCount;
                var writer = new Thread(() =>
                {
                    StartDecompressorWriter(blocksCount, writeWaiter);
                })
                {
                    IsBackground = true
                };
                writer.Start();
                var reader = new Thread(() =>
                {
                    StartDecompressorReader(blocksCount, threadCount, readWaiter);
                })
                {
                    IsBackground = true
                };
                reader.Start();

                var zipper = new Zipper();
                var untilReaderAndWriterNotFinish = writeWaiter.WaitOne(0) == false;
                while (untilReaderAndWriterNotFinish)
                {
                    while (!_inputBlocks.IsEmpty)
                    {
                        var isSuccessRemoved = _inputBlocks.TryRemove(nextIndex, out InputBlock result);
                        if (isSuccessRemoved)
                        {
                            _onProcessingBlockCount++;
                            pool.QueueTask(() =>
                            {
                                var decompressedData = zipper.Decompress(result.Data);
                                decompressedData = decompressedData.Take(result.Length).ToArray();
                                var block = new DataBlock(new BlockHeader(result.Index, result.Length, result.Data.Length), decompressedData);
                                _blocks[block.Header.Index] = block;
                                _onProcessingBlockCount--;
                            });
                            nextIndex++;
                        }
                    }

                    untilReaderAndWriterNotFinish = writeWaiter.WaitOne(0) == false;
                }
            }
            Console.WriteLine($"Decompressed {_config.Input.Name} " +
                              $"from {_config.Input.Length.ToString()} " +
                              $"to {_config.Output.Length.ToString()} bytes.");
        }

        private long DecompressionPrepare()
        {
            long blocksCount;
            try
            {
                var sourceFileLength = GetSourceFileInfo();
                _config.CheckAvailableFreeSpace(_config.Output.FullName, sourceFileLength);
                _config.CalculateBufferSize(sourceFileLength);
                long remainder = sourceFileLength % _config.BufferSize;

                blocksCount = sourceFileLength / _config.BufferSize;
                if (remainder != 0)
                {
                    blocksCount = sourceFileLength / _config.BufferSize + 1;
                }
            }
            catch (ZipperServiceConfigException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new ZipperServiceException(e.Message, e);
            }

            return blocksCount;
        }

        private void StartDecompressorReader(long blocksCount, int threadCount, AutoResetEvent readWaiter)
        {
            var index = 0L;
            using (var reader = _config.Input.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                reader.Position = sizeof(long);
                while (index < blocksCount)
                {
                    if (_inputBlocks.Count >= threadCount)
                    {
                        SpinWait.SpinUntil(() => _onProcessingBlockCount < threadCount);
                    }
                    var hrw = new HeaderReaderWriter();
                    var header = hrw.CreateBlockHeader(reader);
                    var compressedDataBuffer = new byte[header.CompressedDataLength];
                    reader.Read(compressedDataBuffer, 0, header.CompressedDataLength);
                    _inputBlocks[index] =
                        new InputBlock(header.Index, header.DecompressedDataLength, compressedDataBuffer);
                    index++;
                }
            }

            readWaiter.Set();
        }

        private void StartDecompressorWriter(long blocksCount, AutoResetEvent writeWaiter)
        {
            using (var writer = _config.Output.OpenWrite())
            {
                var nextIndex = 0L;
                while (nextIndex < blocksCount)
                {
                    while (_blocks.ContainsKey(nextIndex))
                    {
                        var block = _blocks[nextIndex];
                        writer.Write(block.Data, 0, block.Data.Length);
                        _blocks.TryRemove(nextIndex, out _);
                        nextIndex++;
                    }
                }
            }

            writeWaiter.Set();
        }
        private long GetSourceFileInfo()
        {
            long sourceFileLength = -1L;
            FileStream reader = null;
            try
            {
                reader = _config.Input.OpenRead();
                var sourceFileLengthInfo = new byte[sizeof(long)];
                reader.Read(sourceFileLengthInfo, 0, sourceFileLengthInfo.Length);
                sourceFileLength = BitConverter.ToInt64(sourceFileLengthInfo, 0);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                reader?.Close();
                reader?.Dispose();
            }

            return sourceFileLength;
        }
    }
}
