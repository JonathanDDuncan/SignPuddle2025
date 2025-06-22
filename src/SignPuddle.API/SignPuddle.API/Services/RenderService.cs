namespace SignPuddle.API.Services
{
    public interface IRenderService
    {
        string RenderSignSvg(string fsw);
        string RenderSymbolSvg(string key);
        string RenderSignTextSvg(string fswSequence);
        byte[] GeneratePngImage(string svg);
    }

    public class RenderService : IRenderService
    {
        private readonly IFormatService _formatService;

        public RenderService(IFormatService formatService)
        {
            _formatService = formatService;
        }

        public string RenderSignSvg(string fsw)
        {
            // Generate SVG for a sign using FSW notation
            return _formatService.ConvertFswToSvg(fsw);
        }

        public string RenderSymbolSvg(string key)
        {
            // Generate SVG for a single symbol by key
            // Implementation would create SVG directly from the symbol key
            return $"<svg viewBox='0 0 30 30'><use href='#symbol-{key}'/></svg>";
        }

        public string RenderSignTextSvg(string fswSequence)
        {
            // Generate SVG for a sequence of signs
            // Parse the sequence and generate SVG for each sign
            
            // Example implementation:
            var signs = fswSequence.Split(' ')
                .Where(s => !string.IsNullOrWhiteSpace(s));
                
            var svgParts = new List<string>();
            foreach (var sign in signs)
            {
                svgParts.Add(_formatService.ConvertFswToSvg(sign));
            }
            
            // Combine SVGs with proper layout (vertical or horizontal)
            bool isVertical = _formatService.IsVertical(fswSequence);
            return CombineSvgs(svgParts, isVertical);
        }

        public byte[] GeneratePngImage(string svg)
        {
            // Convert SVG to PNG image bytes
            // In a real implementation, this might use SkiaSharp or another library
            
            // Placeholder implementation:
            return System.Text.Encoding.UTF8.GetBytes(svg);
        }

        #region Helper Methods
        private string CombineSvgs(IEnumerable<string> svgs, bool vertical)
        {
            if (!svgs.Any())
                return "<svg></svg>";

            if (vertical)
            {
                return "<svg class='signtext-vertical'>" + string.Join("", svgs) + "</svg>";
            }
            else
            {
                return "<svg class='signtext-horizontal'>" + string.Join("", svgs) + "</svg>";
            }
        }
        #endregion
    }
}