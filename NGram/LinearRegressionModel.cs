using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NGram
{
    class LinearRegressionModel
    {
        IEnumerable<Point> _inputData = null; //current data set
        private double _b0; //Y Interception (yMean - xMean * _b1)
        private double _b1; //Slope/Gradient of line (Σ (x(difference) * y(difference) / Σ (x(difference) ^ 2)
        private double yMeanDifferenceSquared; //the sum of each difference between the y value and the y mean squared (Σ (y(hat) - y(mean))^2)
        private double xMeanDifferenceSquared; //the sum of each difference between the x value and the x mean squared (Σ (x(hat) - x(mean))^2)
        private double yMean; //the yMean ( (Σy) / N
        private double xMean; //the xMean ( (Σx) / N 

        public void SetDataSet(string data)
        {
            _inputData =  data.Trim().Split('|').Select(x =>
                {
                    var numbers = Regex.Matches(x, @"\d+");
                    var xValue = double.Parse(numbers[0].Value);
                    var yValue = double.Parse(numbers[1].Value);

                    return new Point(xValue, yValue);
                });

            SetRegressionFormula();
        }

        public IEnumerable<Point> DataSet
        {
            get
            {
                return _inputData;
            }
        }

        public string Formula
        {
            get
            {
                return _b0.ToString("#.##") + "+" + _b1.ToString("#.##") + "x";
            }
        }

        /// <summary>
        /// Returns the Y estimate using the generated regression line
        /// </summary>
        /// <param name="xValue">x coord {y = d0 + d1*x}</param>
        /// <returns></returns>
        public double RegressionLineEstimate (double xValue)
        {
            if (_inputData == null)
            {
                throw new InvalidOperationException("Data set has not been set");
            }

            return _b0 + (_b1 * xValue);
        }

        /// <summary>
        /// Works out how close each is from the mean if it was plotted on the regression line
        /// </summary>
        /// <returns></returns>
        public double RSquared()
        {
            var estimate = _inputData.Sum(x => Math.Pow(RegressionLineEstimate(x.X) - yMean, 2));
            return estimate / yMeanDifferenceSquared;
        }

        /// <summary>
        /// Works out how close each value is from the actual value if it was plotted on the regression line
        /// </summary>
        /// <returns></returns>
        public double StandardError()
        {
            var sum = _inputData.Sum(x => Math.Pow(RegressionLineEstimate(x.X) - x.Y, 2));

            return Math.Round(Math.Sqrt(sum / (_inputData.Count() - 2)), 2);
        }

        private void SetRegressionFormula()
        {
            //Simple mean calculation
            xMean = _inputData.Sum(x => x.X) / _inputData.Count();
            yMean = _inputData.Sum(x => x.Y) / _inputData.Count();

            //We now need to calculate the difference between each y and x value in respect to the mean. Square each value and then sum them up
            yMeanDifferenceSquared = _inputData.Sum(x => Math.Pow(x.Y - yMean, 2));
            xMeanDifferenceSquared = _inputData.Sum(x => Math.Pow((x.X - xMean), 2));


            _b1 = _inputData.Sum(x => (x.X - xMean) * (x.Y - yMean)
                / xMeanDifferenceSquared); // Line gradient/slope

            _b0 = yMean - (xMean * _b1); //Y Intercept

        }
    }

    struct Point
    {

        public Point(double _x, double _y) : this ()
        {
            this.X = _x;
            this.Y = _y;
        }

        public double X { get; set; }
        public double Y { get; set; }
    }

    
}
