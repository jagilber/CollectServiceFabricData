// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

using System;
using System.Text;

namespace CollectSFData.Common
{
    public class LogMessage : EventArgs
    {
        private string _message;
        public ConsoleColor? BackgroundColor { get; set; }
        public ConsoleColor? ForegroundColor { get; set; }
        public bool IsError { get; set; }
        public bool LogFileOnly { get; set; }
        public string Message
        {
            get => _message;
            set
            {
                // set fallback for any non printable characters
                _message = Encoding.UTF8.GetString(
                    Encoding.Convert(Encoding.UTF8, Encoding.GetEncoding(
                                Encoding.UTF8.EncodingName,
                                new EncoderReplacementFallback(string.Empty),
                                new DecoderExceptionFallback()
                            ),
                        Encoding.UTF8.GetBytes(value)
                    )
                );
            }
        }
        public string TimeStamp { get; set; }
    }
}