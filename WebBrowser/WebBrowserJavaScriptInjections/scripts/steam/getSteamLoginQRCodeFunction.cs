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

  const cells = container.querySelectorAll('div');
  const gridSize = 41;

  let output = '';
  for (let i = 0; i < cells.length; i++) {
    const style = window.getComputedStyle(cells[i]);
    const bg = style.backgroundColor;

    if (bg === 'rgb(33, 35, 40)') {
      output += '██';  // Black block, double width
    } else if (bg === 'rgb(255, 255, 255)') {
      output += '  ';  // White block, double width space
    } else {
      output += '??';  // Fallback for unknown colors
    }

    if ((i + 1) % gridSize === 0) {
      output += '\n';
    }
  }

  console.log(output);
  return output;
}
";
        }
    }
}
