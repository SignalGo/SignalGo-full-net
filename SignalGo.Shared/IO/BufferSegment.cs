// Licensed to the ali.visual.studio@gmail.com under one or more agreements.
// The license this file to you under the GNU license.
// See the LICENSE file in the project root for more information.
//https://github.com/Ali-YousefiTelori
//https://github.com/SignalGo/SignalGo-full-net
//<PERFORMANCE>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SignalGo.Shared.IO
{
    /// <summary>
    /// save buffer of byte array on memory
    /// manage for memory heap and data for Garbage Collector
    /// </summary>
    public class BufferSegment<T>
    {
        /// <summary>
        /// buffer of this segment
        /// </summary>
        public T[] Buffer { get; set; }
        /// <summary>
        /// Current of position to read
        /// </summary>
        public int Position { get; set; }

        /// <summary>
        /// Is finished read buffer of segment
        /// </summary>
        public bool IsFinished
        {
            get
            {
                return Position == Buffer.Length;
            }
        }

        /// <summary>
        /// Read one byte of Segment from Buffer
        /// </summary>
        /// <returns></returns>
        public T ReadFirstByte()
        {
            T result = Buffer[Position];
            Position++;
            return result;
        }

        /// <summary>
        /// It will tell you what is first byte from current position
        /// </summary>
        /// <returns></returns>
        public T WhatIsFirstByte()
        {
            return Buffer[Position];
        }

        /// <summary>
        /// read buffer from this segment
        /// </summary>
        /// <param name="count">count of need to read</param>
        /// <returns>bytes of segment</returns>
        public T[] ReadBufferSegment(ref int count)
        {
            var result = Buffer.Skip(Position).Take(count).ToArray();
            Position += result.Length;
            return result;
        }

        /// <summary>
        /// read buffer from this segment
        /// </summary>
        /// <param name="exitBytes">break when this bytes found</param>
        /// <param name="isFound">is found exitBytes from buffer</param>
        /// <returns>bytes of segment</returns>
        public IEnumerable<T> Read(ref T[] exitBytes, out bool isFound)
        {
            isFound = false;
            int startPosition = Position;
            var len = exitBytes.Length;
            for (int i = Position; i < Buffer.Length; i++)
            {
                if (Buffer.Skip(i).Take(len).SequenceEqual(exitBytes))
                {
                    isFound = true;
                    Position += len;
                    break;
                }
                Position++;
            }
            return Buffer.Skip(startPosition).Take(Position - startPosition);
        }
    }
}
