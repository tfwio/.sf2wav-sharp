/*
 * tfooo in #DEVLEOP
 * DateTime: 12/27/2006 : 9:04 AM
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

using System.WaveFormat;

namespace on.riffwave.iff_form
{
	public class RiffForm
	{
		SUBCHUNK GetChunk(string key)
		{
			foreach (KeyValuePair<long, SUBCHUNK> i in this.Cks.SubChunks)
				if (i.Value.ckID.ToString4()==key) return i.Value;
			return SUBCHUNK.Empty;
		}
		
		public SUBCHUNK this[string key] { get { return GetChunk(key); } }
		
		public	ChunkCollection Cks {
			get { return cks; } set { cks = value; }
		} ChunkCollection cks;

		public RiffForm(){}
		
		static public RiffForm Load(string path)
		{
			RiffForm riff = default(RiffForm);
			using (Stream fs = File.Open(path,FileMode.Open,FileAccess.Read,FileShare.ReadWrite)) riff = Load(fs);
			return riff;
    }

    static byte[] briff = new byte[4] { (byte)'R', (byte)'I', (byte)'F', (byte)'F' };

    static public RiffForm Load(Stream fs)
		{
			RiffForm riff = new RiffForm();
			ChunkCollection cks = riff.Cks = new ChunkCollection();
			using (BinaryReader bread = new BinaryReader(fs))
			{
				cks.ckMain = new CHUNK(bread);
				cks.SubChunks = new Dictionary<long,SUBCHUNK>();
				
				if (cks.ckMain.ckID == RiffType.RIFF)
				{
					while (fs.Position < fs.Length)
					{
						SUBCHUNK CX = new SUBCHUNK(bread);
						long tpos = fs.Position - Marshal.SizeOf(CX);
						
						cks.SubChunks.Add(tpos,CX);
						
						if (CX.ckID.ToString4() != "    ")
              switch (CX.ckID.ToString4())
						{
							case "fmt ":
								cks.ckFmt = IOHelper.ReadChunk<WaveFormat>(tpos,bread);
								break;
							case "fact":
							case "INFO":
								cks.ckFact = IOHelper.ReadChunk<ChunkFact>(tpos,bread);
								break;
							case "inst":
								cks.ckInst = IOHelper.ReadChunk<_inst>(tpos,bread);
								break;
							case "smpl":
								cks.ckSmpl = IOHelper.ReadChunk<_smp>(tpos,bread);
								List<_smpLoop> bob = new List<_smpLoop>();
								for (int i=0; i<cks.ckSmpl.smpSampleLoops; i++) bob.Add(new _smpLoop(bread));
								// clear the list.
								cks.ckSmpLoop = bob.ToArray();
								bob.Clear();
								bob = null;
								break;
							case "data":
							case "JUNK":
							case "PAD ":
								System.Diagnostics.Debug.Print("Skipping Chunk: '{0}'\n",CX.ckID);
								break;
							default:
								System.Diagnostics.Debug.Print("Unknown Chunk: '{0}'\n",CX.ckID);
//								Console.Beep();
								break;
						}
						fs.Seek(tpos+CX.ckLength+Marshal.SizeOf(CX),SeekOrigin.Begin);
					}
				}
				bread.Close();
			}
			fs.Close();
			riff.Cks = cks;
			return riff;
		}

	}
}
