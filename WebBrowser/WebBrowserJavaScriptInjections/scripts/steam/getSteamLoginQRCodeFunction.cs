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
            return @"function getQRCodeText() {
  const container = document.querySelector('[style*=""grid-template-columns: repeat(41""]');
  if (!container) return 'QR container not found.';

  const cells = Array.from(container.querySelectorAll('div'));
  const gridSize = 41;

  // Create a 2D array of booleans: true = black, false = white
  const boolGrid = [];
  for (let i = 0; i < cells.length; i++) {
    const style = window.getComputedStyle(cells[i]);
    const bg = style.backgroundColor;
    const isBlack = bg === 'rgb(33, 35, 40)';
    const row = Math.floor(i / gridSize);
    const col = i % gridSize;
    if (!boolGrid[row]) boolGrid[row] = [];
    boolGrid[row][col] = isBlack;
  }

  let output = '';
  for (let row = 0; row < gridSize; row += 2) {
    for (let col = 0; col < gridSize; col++) {
      const top = boolGrid[row]?.[col] ? 1 : 0;
      const bottom = boolGrid[row + 1]?.[col] ? 1 : 0;

      let char;
      if (top && bottom) char = '█';
      else if (top && !bottom) char = '▀';
      else if (!top && bottom) char = '▄';
      else char = ' ';
      output += char;
    }
    output += '\n';
  }

  console.log(output);

      window.chrome.webview.postMessage({
        type: ""SteamQRCode"",
        payload: { qrcode: output }
      });
  return output;
}

getQRCodeText();
";
        }
    }
}
