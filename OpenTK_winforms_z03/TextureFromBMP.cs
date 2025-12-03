using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTK_winforms_z03
{
    class TextureFromBMP
    {
        public int id;
        public int width;
        public int height;

        public TextureFromBMP(int _id, int _width, int _height)
        {
            id = _id;
            width = _width;
            height = _height;
        }
    }
}
