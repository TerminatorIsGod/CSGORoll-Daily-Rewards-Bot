using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBrowser.WebBrowserJavaScriptInjections.scripts.steam
{
    class getSteamLoginQRCodeFunction : JavaScriptInjection
    {
        public override string GetJavaScript(Dictionary<string, string> args = null)
        {
            return @"(async () => {
                  const imgElement = Array.from(document.querySelectorAll('img'))
                    .find(img => img.src.startsWith('blob:'));

                  if (!imgElement) {
                    console.error('No <img> element with a blob: URL found.');
                    return;
                  }

                  console.log('Found image:', imgElement);

                  const scale = 4;
                  const canvasScale = 5;

                  const originalWidth = imgElement.naturalWidth;
                  const originalHeight = imgElement.naturalHeight;

                  const scaledWidth = originalWidth * scale;
                  const scaledHeight = originalHeight * scale;

                  const canvasWidth = originalWidth * canvasScale;
                  const canvasHeight = originalHeight * canvasScale;

                  const canvas = document.createElement('canvas');
                  canvas.width = canvasWidth;
                  canvas.height = canvasHeight;

                  const ctx = canvas.getContext('2d');
                
                  ctx.fillStyle = 'white'; 
                  ctx.fillRect(0, 0, canvasWidth, canvasHeight);

                  ctx.imageSmoothingEnabled = false;

                  const x = (canvasWidth - scaledWidth) / 2;
                  const y = (canvasHeight - scaledHeight) / 2;

                  ctx.drawImage(imgElement, 0, 0, originalWidth, originalHeight, x, y, scaledWidth, scaledHeight);

                  const base64String = canvas.toDataURL('image/png'); // or 'image/jpeg'

                  console.log('Base64 Image:', base64String);
                 window.chrome.webview.postMessage({
                  type: ""SteamQRCode"",
                  payload: { qrcode64: base64String}
                });
             })();
";
        }
    }
}
