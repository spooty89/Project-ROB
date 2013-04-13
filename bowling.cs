using System;

namespace AssemblyCSharp
{
	public class bowling
	{
		protected struct frame {
			public int tryOne;
			public int tryTwo;
			public int specialTry;
		}
		
		private frame[] game = new frame[10];
		
		public bowling () {
			for(int i = 0; i < 10; i++) {
				game[i].tryOne = int.Parse(Console.ReadLine());
			}
		}
	}
}

