namespace BlingoEngine.IO.Legacy.Texts
{
    internal class XmedBoundingRectangleReader
    {
        private readonly BlXmedTokenStyleParser _styleParser;
        private readonly XmedDocument _document;

        public XmedBoundingRectangleReader(XmedDocument document, BlXmedTokenStyleParser styleParser)
        {
            _styleParser = styleParser;
            _document = document;
        }

        public void ReadBox(BlXmedTokenReader reader)
        {
            reader.Skip();
            var numbers = new List<int>();
            int nestedDepth = 0;

            while (!reader.IsAtEnd)
            {
                var token = reader.Peek();
                if (token is null) break;

                if (token.IsPrefixedHex02() && token.TryGetNumericValue(out var value))
                {
                    numbers.Add(value);
                    reader.Skip();
                    continue;
                }

                if (token.IsC1())
                {
                    _styleParser.TrackStyleMarker(token);
                    nestedDepth++;
                    reader.Skip();
                    continue;
                }

                if (token.IsC2())
                {
                    nestedDepth++;
                    reader.Skip();
                    continue;
                }

                if (token.IsFieldTerminator())
                {
                    reader.Skip();
                    if (nestedDepth == 0) break;
                    nestedDepth--;
                    continue;
                }

                if (token.IsFieldSeparator() || token.IsBoolean())
                {
                    reader.Skip();
                    continue;
                }

                if (token.IsBlockBoundary()) break;

                reader.Skip();
            }

            if (numbers.Count >= 2)
            {
                long width = numbers[1] - numbers[0];
                if (width < 0) width = 0;
                _document.Width = (uint)width;
            }

            if (numbers.Count >= 4)
            {
                long height = numbers[3] - numbers[2];
                if (height < 0) height = 0;
                _document.Height = (uint)height;
            }
        }
    }
}
