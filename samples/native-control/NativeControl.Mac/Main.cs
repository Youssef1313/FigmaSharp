﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

using AppKit;
using CoreGraphics;
using Foundation;

using FigmaSharp;
using FigmaSharp.Cocoa;
using FigmaSharp.NativeControls.Cocoa;

namespace ExampleFigmaMac
{
    static class MainClass
    {
        static IScrollViewWrapper scrollViewWrapper;
         
        static void Main(string[] args)
        {
            FigmaApplication.Init(Environment.GetEnvironmentVariable("TOKEN"));

            NSApplication.Init();
            NSApplication.SharedApplication.ActivationPolicy = NSApplicationActivationPolicy.Regular;
          
            var xPos = NSScreen.MainScreen.Frame.Width / 2;
            var yPos = NSScreen.MainScreen.Frame.Height / 2;

            var mainWindow = new NSWindow(new CGRect(xPos, yPos, 300, 368), NSWindowStyle.Titled | NSWindowStyle.Resizable | NSWindowStyle.Closable, NSBackingStore.Buffered, false);
            mainWindow.Title = "Cocoa Figma Local File Sample";

            var stackView = new NSStackView() { Orientation = NSUserInterfaceLayoutOrientation.Vertical };
            scrollView = new NSScrollView()
            {
                HasVerticalScroller = true,
                HasHorizontalScroller = true,
                AutomaticallyAdjustsContentInsets = false
            };

            scrollViewWrapper = new ScrollViewWrapper(scrollView);

            stackView.AddArrangedSubview(scrollView);

            scrollView.AutohidesScrollers = false;
            scrollView.BackgroundColor = NSColor.Black;
            scrollView.ScrollerStyle = NSScrollerStyle.Legacy;
            mainWindow.ContentView = stackView;

            var contentView = new NSView { Frame = new CGRect(CGPoint.Empty, mainWindow.Frame.Size) };
            contentView.AutoresizingMask = NSViewResizingMask.WidthSizable | NSViewResizingMask.HeightSizable;

            scrollView.DocumentView = contentView;

			var file = "Dq1CFm7IrDi3UJC7KJ8zVjOt";

            var figmaConverters = Resources.GetConverters()
                .Union (FigmaSharp.AppContext.Current.GetFigmaConverters())
                .ToArray ();

            var manager = new ExampleViewManager(scrollViewWrapper, figmaConverters);
            manager.Initialize();

            mainWindow.MakeKeyAndOrderFront(null);
            NSApplication.SharedApplication.ActivateIgnoringOtherApps(true);
            NSApplication.SharedApplication.Run();
        }

        static NSScrollView scrollView;
    }
}

