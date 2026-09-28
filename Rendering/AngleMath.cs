namespace WheelSpinner.Rendering
{
    public static class AngleMath
    {
         
        public static double Normalize(double degrees)
        {
            return ((degrees % 360) + 360) % 360;
        }
    }
}
