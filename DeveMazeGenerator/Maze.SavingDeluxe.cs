﻿using DeveMazeGenerator.Generators;
using DeveMazeGenerator.InnerMaps;
using Hjg.Pngcs;
using Hjg.Pngcs.Chunks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeveMazeGenerator
{
    public partial class Maze
    {
        /// <summary>
        /// Saves the maze with a specified path as PNG
        /// Uses more memory then saving without a path (depending on the selected MazeSaveType)
        /// </summary>
        /// <param name="fileName">The filename of the file</param>
        /// <param name="path">The path (can be generated by calling PathFinderDepthFirst.GoFind)</param>
        public void SaveMazeAsImageDeluxe(String fileName, List<MazePoint> path, Action<int, int> lineSavingProgress)
        {
            if (lineSavingProgress == null)
            {
                lineSavingProgress = (cur, tot) => { };
            }


            List<MazePointPos> pathPosjes = new List<MazePointPos>(path.Count);


            for (int i = 0; i < path.Count; i++)
            {
                var curPathNode = path[i];
                byte formulathing = (byte)((double)i / (double)path.Count * 255.0);
                var pathPos = new MazePointPos(curPathNode.X, curPathNode.Y, formulathing);
                pathPosjes.Add(pathPos);
            }

            SaveMazeAsImageDeluxe(fileName, pathPosjes, lineSavingProgress);
        }


        /// <summary>
        /// Saves the maze with a specified path as PNG
        /// Uses more memory then saving without a path (depending on the selected MazeSaveType)
        /// </summary>
        /// <param name="fileName">The filename of the file</param>
        /// <param name="path">The path (can be generated by calling PathFinderDepthFirst.GoFind)</param>
        public void SaveMazeAsImageDeluxe(String fileName, List<MazePointPos> pathPosjes, Action<int, int> lineSavingProgress)
        {
            //pathPosjes = pathPosjes.OrderBy(t => t.Y).ThenBy(t => t.X).ToArray();


            pathPosjes.Sort((first, second) =>
            {
                if (first.Y == second.Y)
                {
                    return first.X - second.X;
                }
                return first.Y - second.Y;
            });





            ImageInfo imi = new ImageInfo(this.Width - 1, this.Height - 1, 8, false); // 8 bits per channel, no alpha 
            // open image for writing 
            PngWriter png = FileHelper.CreatePngWriter(fileName, imi, true);
            // add some optional metadata (chunks)
            png.GetMetadata().SetDpi(100.0);
            png.GetMetadata().SetTimeNow(0); // 0 seconds fron now = now
            png.CompLevel = 4;
            //png.GetMetadata().SetText(PngChunkTextVar.KEY_Title, "Just a text image");
            //PngChunk chunk = png.GetMetadata().SetText("my key", "my text .. bla bla");
            //chunk.Priority = true; // this chunk will be written as soon as possible


            int curpos = 0;

            for (int y = 0; y < this.Height - 1; y++)
            {
                ImageLine iline = new ImageLine(imi);

                for (int x = 0; x < this.Width - 1; x++)
                {

                    int r = 0;
                    int g = 0;
                    int b = 0;

                    MazePointPos curPathPos;
                    if (curpos < pathPosjes.Count)
                    {
                        curPathPos = pathPosjes[curpos];
                        if (curPathPos.X == x && curPathPos.Y == y)
                        {
                            r = curPathPos.RelativePos;
                            g = 255 - curPathPos.RelativePos;
                            b = 0;
                            curpos++;
                        }
                        else if (this.innerMap[x, y])
                        {
                            r = 255;
                            g = 255;
                            b = 255;
                        }
                    }
                    else if (this.innerMap[x, y])
                    {
                        r = 255;
                        g = 255;
                        b = 255;
                    }

                    ImageLineHelper.SetPixel(iline, x, r, g, b);
                }
                png.WriteRow(iline, y);
                lineSavingProgress(y, this.Height - 2);
            }
            png.End();
        }
    }
}