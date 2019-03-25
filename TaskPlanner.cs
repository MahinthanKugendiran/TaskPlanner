using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskPlanner.Core
{
    public class TaskPlanner
    {
        public TimeSpan startTime; 
        public TimeSpan stopTime;
        public DateTime taskFinishingDate;
        private string recurringDateFormat = "dd/MM";
        private double workingTimeInSeconds;
        private double workingTimeInHours;
        private const int OneDayInSeconds = 24 * 60 * 60; 

       public  List<DateTime> listOfNormalHolidays = new List<DateTime>();
       public  List<string> listOfRecurringHolidays = new List<string>();

        public  void SetWorkDayStartAndStop(TimeSpan startTime, TimeSpan stopTime)
        {
            this.startTime = startTime;
            this.stopTime = stopTime;

            workingTimeInSeconds = this.stopTime.Subtract(this.startTime).TotalSeconds;
            workingTimeInHours = this.stopTime.Subtract(this.startTime).TotalHours;
        }

        public bool SetHoliday(DateTime dateTime)
        {
            listOfNormalHolidays.Add(dateTime);
            return listOfNormalHolidays.Contains(dateTime);
        }

        public bool SetRecurringHoliday(DateTime dateTime)
        {
            //**recurringDateFormat = "dd/MM"
            //**store DATE/MONTH combination without year as string. 
            //**that will never change. Can check for HOLIDAY or NOT regardless the YEAR factor with date parameter.
            listOfRecurringHolidays.Add(dateTime.ToString(recurringDateFormat));
            return listOfRecurringHolidays.Contains(dateTime.ToString(recurringDateFormat));
        }

        public DateTime GetTaskFinishingDateAndTime(DateTime start, double days)
        {
            var timeOfStart = start.TimeOfDay;//getting exact Time of start date

            if ((0 < days) && (days < 1) && (timeOfStart == startTime))
            {/*this performs find task finishig date only if days value is greater than 0 
                and less than 1.
                And time of start must be as working startTime
            */
                double daysInHours = 24 * days;// (24*0.5=12)
                double num = 24 / workingTimeInHours;// (24/8=3)
                double daysAsWorkingHours = daysInHours / num;// (12/3=4)
                taskFinishingDate = start;//if start is 01/03/2019 8.00 AM 

                //added daysAsWorkingHours here. if daysAsWorkingHours is 4 hrs then it will 01/03/2019 12.00 PM
                //never exceed to stopTime because days is below than 1, So daysAsWorkingHours cannot be greater than 8 hrs.
                taskFinishingDate = taskFinishingDate.AddHours(daysAsWorkingHours);
            }

            else if ((days > 0) && (days < 1) && (timeOfStart == stopTime))
            {/*this performs find task finishig date only if days value is greater than 0 
                and  less than 1.
                And time of start must be as working stopTime
            */

                var timeGap = timeOfStart.Subtract(startTime).TotalHours;//4.00 PM - 8.00 AM is 8 hrs

                taskFinishingDate = start;//if start is 01/03/2019 4.00 PM 
                taskFinishingDate = taskFinishingDate.AddHours((-1) * timeGap);//added - 8 hrs then it will 01/03/2019 8.00 AM
                taskFinishingDate = taskFinishingDate.AddDays(1);//added 1 day then it will 02/03/2019 8.00 AM 

                while (IsHoliday(taskFinishingDate, listOfNormalHolidays, listOfRecurringHolidays))
                {//holiday check for 02/03/2019 8.00 AM 
                    taskFinishingDate = taskFinishingDate.AddDays(1);//will increse if that day is holiday by 1 day
                }

                var daysInHours = 24 * days;// (24*0.5=12)
                var num = 24 / workingTimeInHours;// (24/8=3)
                var daysAsWorkingHours = daysInHours / num;// (12/3=4)

                //if days is .05 then daysAsWorkingHours will be 4 hrs. then added by daysAsWorkingHours.
                taskFinishingDate = taskFinishingDate.AddHours(daysAsWorkingHours); 
            }

            else if ((days > 0) && (days < 1) && (timeOfStart > startTime) && (timeOfStart < stopTime))
            {/*this performs find task finishig date only if days value is greater than 0 
                 and  less than 1.
                 And time of start must be between work starTime and work stopTime
             */
                var daysInHours = 24 * days;// (24*0.5=12)
                var num = 24 / workingTimeInHours;// (24/8=3)
                var daysAsWorkingHours = daysInHours / num;// (12/3=4)

                taskFinishingDate = start;//if start is 01/03/2019 3.00 PM
                taskFinishingDate = taskFinishingDate.AddHours(daysAsWorkingHours);//if daysAsWorkingdays is 4 hours taskfinishingDate is 01/03/2019 7.00 PM

                if (taskFinishingDate.TimeOfDay > stopTime)//here 7.00 PM > 4.00 PM is true
                {
                    var timeOfExceedFromStopTime = taskFinishingDate.TimeOfDay;//here 7.00 PM

                    var timeGapOne = (taskFinishingDate.TimeOfDay).Subtract(startTime).TotalHours;// here 7.00 PM - 8.00 AM is 11 hours
                    taskFinishingDate = taskFinishingDate.AddHours((-1) * timeGapOne);// added -11 hrs then 01/03/2019 8.00 AM
                    taskFinishingDate = taskFinishingDate.AddDays(1);// added 1 day, then 02/03/2019 8.00 AM 

                    var timeGapTwo = timeOfExceedFromStopTime.Subtract(stopTime).TotalHours;//here 7.00 PM - 4.00 PM is 3 hrs
                    taskFinishingDate = taskFinishingDate.AddHours(timeGapTwo);// here added 3 hrs to 02/03/2019 8.00 PM, then it will 02/03/2019 11.00 AM
                }
                while (IsHoliday(taskFinishingDate, listOfNormalHolidays, listOfRecurringHolidays))//here holiday check for 02/03/2019 11.00 AM
                {
                    taskFinishingDate = taskFinishingDate.AddDays(1);//if 02/03/2019 is holiday that will increment untill reach not holiday
                }

                //if not 02/03/2019 is holiday that will be taskFinishingDate with time 11.00 AM
            }

            else if ((days > 0) && (days < 1) && (timeOfStart > stopTime))
            {/*this performs find task finishig date only if days value is greater than 0 
                and  less than 1.
                And time of start must be greater than stopTime
            */

                // if start is 01/03/2019 5.00 PM

                var timeGapOne = timeOfStart.Subtract(startTime).TotalHours;//5.00 PM - 8.00 AM is 9 hrs

                taskFinishingDate = start;// if start is 01/03/2019 5.00 PM
                taskFinishingDate = taskFinishingDate.AddHours((-1) * timeGapOne);//added -9 hrs then it will 01/03/2019 8.00 AM

                //must add by 1 day because start time is greater than stopTime. So, it msut got to next day.
                taskFinishingDate = taskFinishingDate.AddDays(1);//here 02/03/2019 8.00 AM

                while (IsHoliday(taskFinishingDate, listOfNormalHolidays, listOfRecurringHolidays))
                {
                    //holiday check and increment if holiday
                    taskFinishingDate = taskFinishingDate.AddDays(1);
                }

                //if days is 0.5 then daysAsWorkingHours is 4 hrs
                var daysInHours = 24 * days;// (24*0.5=12)
                var num = 24 / workingTimeInHours;// (24/8=3)
                var daysAsWorkingHours = daysInHours / num;// (12/3=4)

                // added by daysAsWorkingHours it will never exceed to stopTime. because days value is below than 1
                taskFinishingDate = taskFinishingDate.AddHours(daysAsWorkingHours);
            }

            else if ((days > 0) && (days < 1) && (timeOfStart < startTime))
            {/*this test performs find task finishig date only if days value is greater than 0 
                and  less than 1.
                And time of start must be smaller than starTime
            */

                //if start is 01/03/2019 7.00 AM

                var timeGapOne = startTime.Subtract(timeOfStart).TotalHours;// 8.00 - 7.00 AM is 1 hr 

                taskFinishingDate = start; // start is 01 / 03 / 2019 7.00 AM
                taskFinishingDate = taskFinishingDate.AddHours(timeGapOne);// it will 01/03/2019 8.00 AM

                var daysInHours = 24 * days;// (24*0.5=12)
                var num = 24 / workingTimeInHours;// (24/8=3)
                var daysAsWorkingHours = daysInHours / num;// (12/3=4)

                //daysAsWorkingHours will added. it never exceed to stopTime of the day. because dyas is below than 1
                taskFinishingDate = taskFinishingDate.AddHours(daysAsWorkingHours);
            }

            else if ((days >= 1) && (timeOfStart > stopTime))
            {/*this performs find task finishig date, 
              only if days value is equal or greater than 1 And 
              time of start must be greater than stopTime 
             */
                taskFinishingDate = start;// 01/03/2019 17.00 

                var decimalDays = Math.Floor(days);//if days 1.75 it will 1
                var remainDays = days - decimalDays;// 1.75 -1.00 = 0.75

                while (decimalDays > 0)
                {
                    //start time is passed to stopTime.
                    //first, must increment by 1 day and then check for holiday 
                    taskFinishingDate = taskFinishingDate.AddDays(1);

                    while (IsHoliday(taskFinishingDate, listOfNormalHolidays, listOfRecurringHolidays))
                    {
                        //holiday checking and day incrementing
                        taskFinishingDate = taskFinishingDate.AddDays(1);
                    }
                    decimalDays--;
                }

                //if here remainDays is 0.75 then daysAsWorkingHours is 6 hrs
                var daysInHours = 24 * remainDays;// (24*0.5=12)
                var num = 24 / workingTimeInHours;// (24/8=3)
                var daysAsWorkingHours = daysInHours / num;// (12/3=4)

                //17.00 - 8.00 
                var TimeGapOne = (taskFinishingDate.TimeOfDay).Subtract(startTime).TotalHours;

                taskFinishingDate = taskFinishingDate.AddHours((-1) * TimeGapOne);//make time of the date to startTime
                taskFinishingDate = taskFinishingDate.AddHours(workingTimeInHours);//set time of that date to stopTime

                //adding remaindays as workinh hours. if remain days 0.75 here 6 hrs will be added.
                //if days is 1 then daysAsWorkingHours will be 0. so time of taskFinishingDate is stopTime(here 16.00)
                //if days greater than 1 daysAsWorkingHours will have value and time of taskFinishingDate will be greater than stopTime 
                taskFinishingDate = taskFinishingDate.AddHours(daysAsWorkingHours);


                if ((taskFinishingDate.TimeOfDay) > stopTime)
                {
                    //getting time gap to set time of that day to starTime (here 8.00 AM)
                    var timeGapThree = (taskFinishingDate.TimeOfDay).Subtract(startTime).TotalHours;

                    //setting time of that day to starTime (here 8.00 AM)
                    taskFinishingDate = taskFinishingDate.AddHours((-1) * timeGapThree);

                    //must go to next day because time of that day above than stopTime 
                    taskFinishingDate = taskFinishingDate.AddDays(1);

                    //setting exact taskFinishingDate with time
                    taskFinishingDate = taskFinishingDate.AddHours(daysAsWorkingHours);

                    while (IsHoliday(taskFinishingDate, listOfNormalHolidays, listOfRecurringHolidays))
                    {
                        //again holiday checking needed, becuase we added days again for remain days 
                        taskFinishingDate = taskFinishingDate.AddDays(1);
                    }
                }
            }

            else if ((days >= 1) && (timeOfStart < startTime))
            {/*this performs find task finishig date, 
              only if days value is equal or greater than 1 
              And time of start must be smaller than startTime 
            */
                taskFinishingDate = start;

                var decimalDays = Math.Floor(days);// dyas 6.75 will be 6
                var remainDays = days - decimalDays;//6.75-6 = 0.75 

                while (decimalDays > 0)
                {
                    //incrementing by 1 day
                    taskFinishingDate = taskFinishingDate.AddDays(1);

                    while (IsHoliday(taskFinishingDate, listOfNormalHolidays, listOfRecurringHolidays))
                    {
                        //holiday checking 
                        taskFinishingDate = taskFinishingDate.AddDays(1);
                    }
                    
                  

                    decimalDays--;
                }

                //getting timeGap for set time of that taskFninishing date to stopTime
                var timeGapOne = stopTime.Subtract(taskFinishingDate.TimeOfDay).TotalHours;

                //setting time of that taskFinishingDate to stopTime
                taskFinishingDate = taskFinishingDate.AddHours(timeGapOne);


                //must consider that start day into working days. but end of the while loop process that sets one additional day to taskFinishingDate
                /*exmaple: if days = 1, start is 01/03/2019 7.00 AM. 
                              after while loop that will 02/03/2019. 
                              but actually 01/03/2019 requires.
                */
                //So, setting to one day before from current taskFinishingDate. 
                taskFinishingDate = taskFinishingDate.AddDays((-1));

                var daysInHours = 24 * remainDays;// (24*0.5=12)
                var num = 24 / workingTimeInHours;// (24/8=3)
                var daysAsWorkingHours = daysInHours / num;// (12/3=4)

                //taskfinishingDate time will be grater than stopTime if remaiDays has value
                taskFinishingDate = taskFinishingDate.AddHours(daysAsWorkingHours);

                //check holiday and day incrementing needed for that remainDays
                if ((taskFinishingDate.TimeOfDay) > stopTime)
                {
                    //getting timeGap to set starTime to that current taskFinishingDate
                    var timeGapTwo = taskFinishingDate.TimeOfDay.Subtract(startTime).TotalHours;

                    //setting time of TaskFinishingDate to startTime
                    taskFinishingDate = taskFinishingDate.AddHours((-1) * timeGapTwo);

                    //adding 1 day more, because if remainDays then must consider that days into next day
                    taskFinishingDate = taskFinishingDate.AddDays(1);

                    //setting exact time to taskfinishingDate
                    taskFinishingDate = taskFinishingDate.AddHours(daysAsWorkingHours);

                    while (IsHoliday(taskFinishingDate, listOfNormalHolidays, listOfRecurringHolidays))
                    {
                        //holiday checking and days incrementing
                        taskFinishingDate = taskFinishingDate.AddDays(1);
                    }
                }
            }

            else if ((days >= 1) && (timeOfStart >= startTime) && (timeOfStart <= stopTime))
            {/*this performs to find task finishing date when days equal or greater than 1 
                and time of start date is must be equals or greater than working startTime and 
                equals or smaller than working stopTime
            */
                taskFinishingDate = start;

                var decimalDays = Math.Floor(days); //if days is 2.25 it will 2
                var remainDays = days - decimalDays;// 2.25 - 2 ==  0.25

                while (decimalDays > 0)
                {
                    taskFinishingDate = taskFinishingDate.AddDays(1);

                    while (IsHoliday(taskFinishingDate, listOfNormalHolidays, listOfRecurringHolidays))
                    {
                        //holiday checking and day incrementng
                        taskFinishingDate = taskFinishingDate.AddDays(1);
                    }
                    decimalDays--;
                }

                var daysInHours = 24 * remainDays;// (24*0.5=12)
                var num = 24 / workingTimeInHours;// (24/8=3)
                var daysAsWorkingHours = daysInHours / num;// (12/3=4)

                //after adding dyasAsWorkingHours, 
                //if this taksFinishingDate time below or equal to stopTime, no nee  add next day
                taskFinishingDate = taskFinishingDate.AddHours(daysAsWorkingHours);

                if ((taskFinishingDate.TimeOfDay) > stopTime)
                {
                    //getting timeGap for setting exact time for taskFinishingDate
                    var TimeGapOne = (taskFinishingDate.TimeOfDay).Subtract(stopTime).TotalHours;

                    //getting timeGap for seeting time of that taskFinishingDate to startTime
                    var timeGapTwo = (taskFinishingDate.TimeOfDay).Subtract(startTime).TotalHours;

                    //setting time of taskFinishingDate to startTime 
                    taskFinishingDate = taskFinishingDate.AddHours((-1) * timeGapTwo);

                    //setting taskFinishingDate to next day
                    //because time of taskFinishingDate is exceeded to stopTime
                    taskFinishingDate = taskFinishingDate.AddDays(1);

                    //must check holiday again, because we again set taskFinishingDate to next day
                    while (IsHoliday(taskFinishingDate, listOfNormalHolidays, listOfRecurringHolidays))
                    {
                        
                        taskFinishingDate = taskFinishingDate.AddDays(1);
                    }

                    //setting exact time for taskFinishingDate 
                    taskFinishingDate = taskFinishingDate.AddHours(TimeGapOne);
                }
            }

            else
            {
                throw new System.ArgumentException("Parameters and some values are out of range from conditions");
            }

            return taskFinishingDate;
        }

        //this is for holiday check
        //if any Saturday, Sunday, Normal Holiday, RecurringHoliday available then return true.
        public bool IsHoliday(DateTime testDate, List<DateTime> listOfNmlHoliday, List<string> listOfRngHoliday)
        {
            return (testDate.DayOfWeek == DayOfWeek.Saturday ||
                testDate.DayOfWeek == DayOfWeek.Sunday ||
                listOfNormalHolidays.Contains(testDate) ||
                listOfRecurringHolidays.Contains(testDate.ToString(recurringDateFormat)));
        }
    }
}
