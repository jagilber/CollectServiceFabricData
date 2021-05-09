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
                _message = Encoding.Unicode.GetString(
                    Encoding.Convert(Encoding.Unicode, Encoding.GetEncoding(
                                Encoding.Unicode.EncodingName,
                                new EncoderReplacementFallback(string.Empty),
                                new DecoderExceptionFallback()
                            ),
                        Encoding.Unicode.GetBytes(value)
                    )
                );
            }
        }
        public string TimeStamp { get; set; }
    }
}