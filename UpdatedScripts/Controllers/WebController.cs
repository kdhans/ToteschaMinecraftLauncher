﻿using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToteschaMinecraftLauncher.UpdatedScripts.Controllers
{
    public class WebController
    {
        internal async Task<ToteschaHttpResponse<ImageTexture>> GetImageDataAsync(string url)
        {
            throw new NotImplementedException();
        }

        internal async Task<ToteschaHttpResponse<T>> GetJsonWebRequestAsync<T>(string url)
        {
            throw new NotImplementedException();
        }
    }
}
