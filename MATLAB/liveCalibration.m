clearvars; clc; close all;

%% Parameters setting
clear device

port = "COM5"; 
baudrate = 115200;
NumberOfSignals = 14;
FirstSignalIdx = 1;

device = serialport(port,baudrate);
configureTerminator(device,'CR/LF');

fs = 20;
windowLength = 1;
counter = 0;
overlapFactor = 5;
smoothingFactor = 0.20;
Ntrials = 3;
dt = 1; % each calibration window length
Nsamples = round(dt*fs);
Ngestures = 4; % 4 pinches
thresholds = zeros(1,5);


%% Begin recording
% Follow the instructions in the command window
for k = 1:Ngestures
    record=[];
    disp(strcat("Pinch",' ',num2str(k+1),' ','(index finger), press a key to start:'))
    waitforbuttonpress
    
    for j = 1:Ntrials
    
        data = [];
        for i= 1:Nsamples
        
            % Reading data from Arduino
            data_str = readline(device);
            data_str_split = split(data_str,',')';
        
            if size(data_str_split,2) < NumberOfSignals
                data = [data; nan(1,NumberOfSignals-FirstSignalIdx+1)];
            else
                data_trim = data_str_split(FirstSignalIdx:NumberOfSignals);
                data = [data; str2double(data_trim)];
            end
            flush(device);
        end
        
        % Compute features
        temp = [range(data(:,1)) range(data(:,2)) range(data(:,3)) min(data(:,10)) min(data(:,11)) min(data(:,12)) min(data(:,13)) min(data(:,14))];
        record = [record; temp];
        
        % Wait for the user
        disp(strcat("Record n.",num2str(j),' ','executed, press a key to continue:'))
    
        if j<Ntrials
            waitforbuttonpress
        end
    end
    
    thresholds(k+1) = median(record(:,k+4));

    if k == 4
        thresholds(1) = median(record(:,4));
    end

    disp(strcat('Threshold P',' ',num2str(k+1),' = ', num2str(thresholds(k+1))))
end

%% Add tolerance and save
% A tolerance is added to make easier the gesture selection
toll = [1.1, 1.1, 1.1, 1.1, 1.2];
thresholds = thresholds.*toll;
save('.\otherSavedFiles\Thresholds_S3','thresholds') 
