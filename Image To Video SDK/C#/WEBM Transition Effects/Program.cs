//*******************************************************************************************//
//                                                                                           //
// Download Free Evaluation Version From: https://bytescout.com/download/web-installer       //
//                                                                                           //
// Also available as Web API! Free Trial Sign Up: https://secure.bytescout.com/users/sign_up //
//                                                                                           //
// Copyright © 2017-2018 ByteScout Inc. All rights reserved.                                 //
// http://www.bytescout.com                                                                  //
//                                                                                           //
//*******************************************************************************************//


using System;
using System.Diagnostics;
using BytescoutImageToVideo;

namespace TransitionEffects
{
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
            	Console.WriteLine("Converting JPG slides to video, please wait...");

				// Create BytescoutImageToVideoLib.ImageToVideo object instance
				ImageToVideo converter = new ImageToVideo();

				// Activate the component
				converter.RegistrationName = "demo";
				converter.RegistrationKey = "demo";

				// Enable transition effects for the first and last slide
				converter.UseInEffectForFirstSlide = true;
				converter.UseOutEffectForLastSlide = true;

				// Add images and set slide durations and transition effects
				Slide slide;
				slide = converter.AddImageFromFileName("..\\..\\..\\..\\slide1.jpg");
				slide.InEffect = TransitionEffectType.teFade;
				slide.OutEffect = TransitionEffectType.teFade;
				slide.Duration = 3000; // 3000ms = 3s
				slide = converter.AddImageFromFileName("..\\..\\..\\..\\slide2.jpg");
				slide.Duration = 3000;
				slide.InEffect = TransitionEffectType.teWipeLeft;
				slide.OutEffect = TransitionEffectType.teWipeRight;
				slide = converter.AddImageFromFileName("..\\..\\..\\..\\slide3.jpg");
				slide.Duration = 3000;
				slide.InEffect = TransitionEffectType.teWipeLeft;
				slide.OutEffect = TransitionEffectType.teWipeRight;
				
				// Set output video size
				converter.OutputWidth = 640;
				converter.OutputHeight = 480;

				// Set output video file name
				converter.OutputVideoFileName = "result.webm";

				// Run the conversion
				converter.RunAndWait();

				// Open the result video file in default webm player
				Process.Start("result.webm");

				Console.WriteLine("Done. Press any key to continue...");
				Console.ReadKey();
            }
            catch(Exception e)
			{
                Console.WriteLine("Error: " + e.ToString());
                Console.WriteLine("\nPress any key to exit.");
                Console.ReadKey();
            }
		}
	}
}
