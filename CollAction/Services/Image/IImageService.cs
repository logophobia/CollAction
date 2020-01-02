﻿using System;
using System.Threading;
using System.Threading.Tasks;
using CollAction.Models;
using Microsoft.AspNetCore.Http;

namespace CollAction.Services.Image
{
    public interface IImageService : IDisposable
    {
        Task<ImageFile> UploadImage(IFormFile fileUploaded, string imageDescription, CancellationToken token);

        Task DeleteImage(ImageFile imageFile, CancellationToken token);

        string GetUrl(ImageFile imageFile);

        void InitializeDanglingImageJob();
    }
}