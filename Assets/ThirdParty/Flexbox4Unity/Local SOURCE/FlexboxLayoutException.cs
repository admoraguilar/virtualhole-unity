using System;

namespace Flexbox4Unity
{
	/**
  * Enables us to catch edge-cases that are impossible to layout with a particular Algorithm implementation, and have
  * the algorithm detect them at top-level and - in some cases - automatically handle them (or report them)
  */
	public class FlexboxLayoutException : Exception
	{
		//
		// For guidelines regarding the creation of new exception types, see
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
		// and
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
		//

		public FlexboxLayoutException()
		{
		}

		public FlexboxLayoutException(string message) : base(message)
		{
		}

		public FlexboxLayoutException(string message, Exception inner) : base(message, inner)
		{
		}
	}
}