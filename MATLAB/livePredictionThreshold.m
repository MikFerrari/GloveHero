clearvars; clc; close all;

%% Load thresholds saved during calibration
load('.\otherSavedFiles\Thresholds_S3.mat') % Select the desired subject

%% Arduino spec
port = "COM5"; 
baudrate = 115200;
NumberOfSignals = 14;
FirstSignalIdx = 1;

device = serialport(port,baudrate);
configureTerminator(device,'CR/LF');

data = [];
i = 0;

ax = gca;
ButtonHandle = uicontrol('Style', 'PushButton', ...
                         'String', 'STOP', ...
                         'Position', [10 10 40 25], ...
                         'Callback', 'delete(gcbf)', ...
                         'BackgroundColor', 'r', ...
                         'ForegroundColor', 'w', ...
                         'FontWeight', 'bold');

%% Read data
fs = 20;
windowLength = 0.5;
Nsamples = round(windowLength*fs);
counter = 0;
overlapFactor = 5;
smoothingFactor = 0.20;
offset = zeros(1,NumberOfSignals);
classes = {'P2','P3','P4','P5','F','S','N'};


%%
tic
while ishandle(ButtonHandle)
    counter = counter + 1;

    % Reading data from Arduino
    data_str = readline(device);
    data_str_split = split(data_str,',')';

    if size(data_str_split,2) < NumberOfSignals
        data = [data; nan(1,NumberOfSignals-FirstSignalIdx+1)];
    else
        data_trim = data_str_split(FirstSignalIdx:NumberOfSignals);
        data = [data; str2double(data_trim)];
    end

    flush(device);  % Clean the serial port from accumulated signals

    if counter >= ceil(Nsamples/overlapFactor)

        % Compute features
          sampleTest = [mean(data(:,1)) mean(data(:,2)) mean(data(:,3)) min(data(:,10)) min(data(:,11)) min(data(:,12)) min(data(:,13)) min(data(:,14))];

        % Print features values if necessary
%         sampleTest
%         sampleTest(1:3) % xyz accelerations
%         sampleTest(4:8) % fingers flexion
  

        % Detect the gesture
        if sampleTest(1) > 0 || sampleTest(2) > 0
            prediction = 'S'
        elseif [sampleTest(5) sampleTest(7) sampleTest(8)] < [thresholds(2) thresholds(4) thresholds(5)]
            prediction = 'F'
        elseif sampleTest(5) < thresholds(2)
            prediction = 'P2'
        elseif sampleTest(6) < thresholds(3)
            prediction = 'P3'    
        elseif sampleTest(7) < thresholds(4)
            prediction = 'P4'
        elseif sampleTest(8) < thresholds(5)
            prediction = 'P5'
        else
            prediction = 'N'
        end

        data = data(ceil(size(data,1)/overlapFactor):end,:); % Keep just the last part to obtain overlapping windows
        
        % Reset values
        counter = 0;
        sampleTest = [];
    end
end
clear device