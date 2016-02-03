using System;
using System.Diagnostics;
using System.IO;

namespace DownloaderUtil
{
    public class DownloaderEventArgs : EventArgs
    {
	public string FilmName {get;set;}
	public string Url {get;set;}
	public decimal Progress {get;set;}
    }
}
