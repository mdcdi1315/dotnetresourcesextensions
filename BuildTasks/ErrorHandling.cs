
using System;
using Microsoft.Build.Framework;

namespace DotNetResourcesExtensions.BuildTasks
{
    internal static class ErrorHandler
    {
        public enum MessageType : System.Byte
        {
            Message,
            Warning,
            Error,
            Critical
        }

        /// <summary>
        /// A stored type that saves all required data to create and show a message in MSBuild. <br />
        /// Note: The messages stored here are format strings that are expanded at run-time. Be careful!.
        /// </summary>
        public sealed class MessagePiece
        {
            private readonly System.String message;
            private readonly System.UInt32 code;
            private readonly MessageType type;

            public MessagePiece(System.UInt32 code, System.String message, MessageType msgtype)
            {
                this.message = message;
                this.code = code;
                this.type = msgtype;
            }

            public System.String Code => $"{BuildTasksErrorCodePrefix}{code:d4}";

            public System.UInt32 NumericCode => code;

            public MessageType Type => type;

            public System.String Message => message;
        }

        private const System.Int32 SpecialErrorCode = 7777;
        private const System.String BuildTasksErrorCodePrefix = "DNTRESEXT";
        private static readonly System.String SpecialErrorCodeString;
        private static readonly MessagePiece[] messages;

        static ErrorHandler()
        {
            SpecialErrorCodeString = $"{BuildTasksErrorCodePrefix}{SpecialErrorCode}";
            // Here in this array all messages are temporarily saved.
            messages = new MessagePiece[] {
                new(0 , "An unexpected exception has occured during code execution: \n{0}" , MessageType.Critical),
                new(1 , "A target output file was not specified. Please specify a valid path , then retry. " , MessageType.Error),
                new(2, "No input files were supplied. Please check whether all files are supplied correctly." , MessageType.Error),
                new(3 , "Unknown output file format specified: {0}" , MessageType.Error),
                new(4 , "First input file should NOT BE NULL AT THIS POINT FAILURE OCCURED" , MessageType.Critical),
                new(5 , "Primary file for reading must always be valid. Resource generation stopped." , MessageType.Error),
                new(6 , "The strongly-typed resource class generation for item {0} has failed due to an unhandled {1}.\nAs a result , the final compilation might fail if your code depends on this class generation. \n{2}" , MessageType.Warning),
                new(7 , "The value specified , {0} was not accepted because of an {1}: \n {2} ." , MessageType.Warning),
                new(8 , "Setting the OutputFileType back to Resources due to an error. See the previous message for more information." , MessageType.Warning),
                new(9 , "The OutputType property must not be empty." , MessageType.Error),
                new(10 , "The Inputs property must contain a valid list of file paths specified in GeneratableResource item." , MessageType.Error),
                new(11 , "The file {0} is not existing." , MessageType.Error),
                new(12 , "Not all source files were found. All files must have been previously declared in the GeneratableResource item. (Found {0} files while expecting {1} files)" , MessageType.Error),
                new(40 , "Cannot generate code because the {0} property in {1} was not specified." , MessageType.Error),
                new(41 , "Cannot generate code because a valid output path for {0} was not specified. Please specify one and retry." , MessageType.Error),
                new(42 , "The strongly typed-class language was not specified. Presuming that it is 'CSharp'." , MessageType.Warning),
                new(43 , "The strongly typed-class .NET name was not specified. Presuming that it is the manifest name: \"{0}\"" , MessageType.Warning),
                new(44 , "The resource class visibility was set to an invalid value. Presuming that it's value is 'Internal'." , MessageType.Warning),
                new(118 , "The specified reader is still on a test phase. Do not trust this reader for saving critical resources." , MessageType.Warning),
                new(119 , "The {0} resource reader has been deprecated and will be removed in a subsequent release. Move to a more stable reader to save your resources." , MessageType.Warning),
                new(213 , "Could not find a resource reader for the file {0}. Resource generation for this item will be skipped. This may cause compilation or run-time errors." , MessageType.Warning),
                new(263 , "An internal exception has been detected in one of the BuildTasks task code. See the next error for more information." , MessageType.Critical)
            };
        }

        /// <summary>
        /// Throws the specified internal error message back to MSBuild , based it's code and severity of issue.
        /// </summary>
        /// <param name="task">The task object from which this message will be thrown to.</param>
        /// <param name="code">The unique code of the message to throw.</param>
        /// <param name="objects">Any formatting arguments that must be passed before the message is thrown.</param>
        public static void ThrowMessage(this Microsoft.Build.Utilities.Task task, System.UInt32 code, params System.Object[] objects)
        {
            foreach (MessagePiece piece in messages)
            {
                if (piece.NumericCode == code)
                {
                    switch (piece.Type)
                    {
                        case MessageType.Message:
                            task.Log.LogMessage(MessageImportance.High, piece.Message, objects);
                            break;
                        case MessageType.Warning:
                            task.Log.LogWarning("", piece.Code, "", "", "<Non-Existent>", 0, 0, 0, 0, piece.Message, objects);
                            break;
                        case MessageType.Error:
                            task.Log.LogError("", piece.Code, "", "", "<Non-Existent>", 0, 0, 0, 0, piece.Message, objects);
                            break;
                        case MessageType.Critical:
                            task.Log.LogCriticalMessage("", piece.Code, "", "<Non-Existent>", 0, 0, 0, 0, piece.Message, objects);
                            break;
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// Throws the internal error 263 followed by the given exception that is a critical exception and signifies that the build must stop.
        /// </summary>
        /// <param name="task">The task object from which this message will be thrown to</param>
        /// <param name="exception">The critical exception to throw.</param>
        public static void LogExceptionClass(this Microsoft.Build.Utilities.Task task, System.Exception exception)
        {
            ThrowMessage(task, 263);
            switch (exception)
            {
                case InvalidInputFromUserException:
                    task.Log.LogError("", SpecialErrorCodeString, "", "", "<Non-Existent>", 0, 0, 0, 0, "Invalid argument inside a task call made the task {0} to fail. \nException: {1}", task.GetType().Name, exception);
                    break;
                case UnexpectedErrorException:
                    task.Log.LogCriticalMessage("", SpecialErrorCodeString, "", "<Non-Existent>", 0, 0, 0, 0, "An unexpected hard error made the task {0} to fail (and consequently fail the build engine). \nException: {1}", task.GetType().Name, exception);
                    break;
                case AggregateException:
                    task.Log.LogError("", SpecialErrorCodeString, "", "", "<Non-Existent>", 0, 0, 0, 0, "One or more hard exceptions made the task {0} to fail. \nException: {1}", task.GetType().Name, exception);
                    break;
                default:
                    task.Log.LogCriticalMessage("", SpecialErrorCodeString, "", "<Non-Existent>", 0, 0, 0, 0, "[INTERNAL ERROR]: Cannot recognize exception type {0} .", exception.GetType().FullName);
                    break;
            }
        }

    }
}
