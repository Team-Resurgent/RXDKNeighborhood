namespace RXDKNeighborhood.Controls
{
    public class DriveSpacePieDrawable : IDrawable
    {

        private ulong mUsedBytes = 0;
        private Color mUsedBytesColor;
        private ulong mFreeBytes = 0;
        private Color mFreeBytesColor;

        public DriveSpacePieDrawable(ulong usedBytes, Color usedBytesColor, ulong freeBytes, Color freeBytesColor)
        {
            mUsedBytes = usedBytes;
            mUsedBytesColor = usedBytesColor;
            mFreeBytes = freeBytes;
            mFreeBytesColor = freeBytesColor;
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            if (mFreeBytes == 0 && mUsedBytes == 0)
            {
                return;
            }


            float usedPercentage = mUsedBytes / (float)(mFreeBytes + mUsedBytes);
            float freePercentage = mFreeBytes / (float)(mFreeBytes + mUsedBytes);
            float[] percentages = { usedPercentage, freePercentage };
            var colors = new Color[] { mUsedBytesColor, mFreeBytesColor };

            var centerX = dirtyRect.Width / 2;
            var centerY = (dirtyRect.Height / 2) - 10;
            var radiusX = dirtyRect.Width / 2;
            var radiusY = (dirtyRect.Height / 2) - 10;

            float startAngle = 180f;
            for (int i = 0; i < 2; i++)
            {
                float offsetY = (10 - (i * 10)) + 10;
                for (int j = 0; j < percentages.Length; j++)
                {
                    var sweepAngle = percentages[j] * 360;

                    var path = new PathF();
                    path.MoveTo(centerX, centerY + offsetY);
                    path.AddArc(centerX - radiusX, (centerY - radiusY) + offsetY, radiusX * 2, (radiusY * 2) + offsetY, startAngle, startAngle + sweepAngle, false);
                    path.LineTo(centerX, centerY + offsetY);
                    canvas.FillColor = colors[j];
                    canvas.FillPath(path);
                    canvas.StrokeColor = Colors.Black;
                    canvas.StrokeSize = 1;
                    canvas.DrawPath(path);

                    startAngle += sweepAngle;
                }
            }
        }


    }
}
