clearvars; close all; clc;

%% Define acquisition parameters.
Year = 2022; Month = 01; Day = 05; % Date

subject = "S3"; % Subject
recordNumber = 1; % It is the number tha identify each acquisition, change it before starting a new one.

port = "COM3"; 
baudrate = 115200;
NumberOfSignals = 14;
FirstSignalIdx = 1;

%%  Define the device
device = serialport(port,baudrate);
configureTerminator(device,'CR/LF');
i = 0;

r = rateControl(frequency); % Adjusts the frequency of execution of the for loop for each acquisition.
reset(r);

ax = gca;
ButtonHandle = uicontrol('Style', 'PushButton', ...
                         'String', 'STOP', ...
                         'Position', [10 10 40 25], ...
                         'Callback', 'delete(gcbf)', ...
                         'BackgroundColor', 'r', ...
                         'ForegroundColor', 'w', ...
                         'FontWeight', 'bold'); % Create a STOP button to end the acquisition

%% Record data
tic
data = [];
while ishandle(ButtonHandle)
    data_str = readline(device);
    data_str_split = split(data_str,',')';

    if size(data_str_split,2) < NumberOfSignals
        data = [data; nan(1,NumberOfSignals-FirstSignalIdx+1)];
    else
        data_trim = data_str_split(FirstSignalIdx:NumberOfSignals);
        data = [data; str2double(data_trim)];
    end

    if false % Define this as "true" for real-time plotting
        if i>140
            plot(ax,data(end-100:end,1:(NumberOfSignals-FirstSignalIdx+1)))
        else
            plot(ax,data(:,1:(NumberOfSignals-FirstSignalIdx+1)))
        end
    end

    drawnow

    i = i+1;
    flush(device); % Clean the serial port from accumulated signals
    waitfor(r);
end

t = toc;

data(1,:) = []; % Delete first line of NaN

% Double check of acquisition frequency
freq = i/t % Compute mean frequency (Method 1)

stats = statistics(r);
freq_r = 1/stats.AveragePeriod % Find mean frequency using rateControl statistics (Method 2)

%% Create timetable
data = array2timetable(data,'TimeStep',seconds(1/freq));
data.Properties.VariableNames = {'IMU1','IMU2','IMU3','IMU4','IMU5','IMU6','IMU7','IMU8','IMU9','Finger1','Finger2','Finger3','Finger4','Finger5'};

%% Save raw data
filename = strcat(string(Year),'_',string(Month),'_',string(Day),'_',subject,'_N',string(recordNumber));
directory = ".\rawData\";
save(strcat(directory,filename),'data') % Save original data in the "rawData" folder

%% Preprocess data and save
dataPrep = preprocess_data(data, filename, 1); % Preprocess data and save in "preprocessedData" folder