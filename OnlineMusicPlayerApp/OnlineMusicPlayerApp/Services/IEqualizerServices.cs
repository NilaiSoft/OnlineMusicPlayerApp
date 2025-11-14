using System;
using System.Collections.Generic;
using System.Text;

namespace OnlineMusicPlayerApp.Services
{
    public interface IEqualizerService
    {
        void Init();
        void SetBand(short band, short level);
        short GetBandCount();
        int GetBandFreq(short band);
    }
}
