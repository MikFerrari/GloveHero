function data = preprocess_data(data, filename, plotFlag)
% OUTPUT data: timetable

% Remove NaNs (caused by flush(device))
data = fillmissing(data,"linear"); 

% Smoothing
smoothingFactor = 0.20;
data = smoothdata(data,'movmean','SmoothingFactor',smoothingFactor);

% Vertical shifting: remove initial offset
data.IMU1 = data.IMU1 - data.IMU1(1);
data.IMU2 = data.IMU2 - data.IMU2(1);
data.IMU3 = data.IMU3 - data.IMU3(1);
data.IMU4 = data.IMU4 - data.IMU4(1);
data.IMU5 = data.IMU5 - data.IMU5(1);
data.IMU6 = data.IMU6 - data.IMU6(1);
data.IMU7 = data.IMU7 - data.IMU7(1);
data.IMU8 = data.IMU8 - data.IMU8(1);
data.IMU9 = data.IMU9 - data.IMU9(1);
data.Finger1 = data.Finger1 - data.Finger1(1);
data.Finger2 = data.Finger2 - data.Finger2(1);
data.Finger3 = data.Finger3 - data.Finger3(1);
data.Finger4 = data.Finger4 - data.Finger4(1);
data.Finger5 = data.Finger5 - data.Finger5(1);

% Plot fingers
if plotFlag == 1
    s = stackedplot(data(:,10:14));
    grid on
    for i = 1:5
%         s.AxesProperties(i).YLimits = [-5 3]; % Used when normalizing
        s.AxesProperties(i).YLimits = [-500 200]; % Used when NOT normalizing
    end
end

% Saving processed data
dataPrep = data;
save(strcat(".\preprocessedData\",filename),'dataPrep')