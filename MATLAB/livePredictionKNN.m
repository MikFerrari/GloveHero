clc; close all;
clear device
%% Load the model
KNN = load('.\otherSavedFiles\KNN.mat'); KNN = KNN.model;

%% Build a new KNN (useful for Arduino implementation)
% load('dataTrain.mat')
% KNN = model;
% meanTrain = mean(table2array(dataTrain(:,1:end-1)));
% stdTrain = std(table2array(dataTrain(:,1:end-1)));

%{
load("dataTrain.mat")

meanTrain = mean(table2array(dataTrain(:,1:end-1)));
stdTrain = std(table2array(dataTrain(:,1:end-1)));

temp = array2table((table2array(dataTrain(:,1:end-1))-meanTrain)./stdTrain);
temp.Properties.VariableNames = {'xRange'; 'yRange'; 'zRange'; 'Finger1Min'; 'Finger2Min'; 'Finger3Min'; 'Finger4Min'; 'Finger5Min'};
dataTrain = [temp table(dataTrain.Label)];
dataTrain.Properties.VariableNames{9} = 'Label';

inputTable = dataTrain;
predictorNames = {'xRange', 'yRange', 'zRange', 'Finger1Min', 'Finger2Min', 'Finger3Min', 'Finger4Min', 'Finger5Min'};
predictors = inputTable(:, predictorNames);
response = inputTable.Label;

KNN = fitcknn(...
    predictors, ...
    response, ...
    'Distance', 'euclidean', ...
    'Exponent', [], ...
    'NumNeighbors', 7, ...
    'DistanceWeight', 'Equal', ...
    'Standardize', false, ...
    'ClassNames', categorical({'P2'; 'P3'; 'P4'; 'P5'; 'N'; 'F';'S'}, {'P2' 'P3' 'P4' 'P5' 'N' 'F' 'S'}));
%}
%% Define arduino parameters
port = "COM3"; 
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

%% Record data
fs = 20;
windowLength = 1;
Nsamples = round(windowLength*fs);
counter = 0;
overlapFactor = 5;
flag= 0;
smoothingFactor = 0.20;
offset = zeros(1,NumberOfSignals);
classes = {'P2','P3','P4','P5','F','S','N'};

tic
while ishandle(ButtonHandle)
    counter = counter + 1;
    % Reading data from Arduino
    data_str = readline(device);
    data_str_split = split(data_str,',')';

    if size(data_str_split,2) < NumberOfSignals
        data = [data; nan(1,NumberOfSignals-FirstSignalIdx+1)];
    else
        if flag == 0 && counter == 3
            offset = data(2,:);
            flag = 1;
        end
        data_trim = data_str_split(FirstSignalIdx:NumberOfSignals);
        data = [data; str2double(data_trim)-offset];
    end


    flush(device);

    if counter >= ceil(Nsamples/overlapFactor)

        % Smoothing data
        data = smoothdata(data,'movmean','SmoothingFactor',smoothingFactor);

        % Compute features
        sampleTest = [mean(data(:,1)) mean(data(:,2)) mean(data(:,3)) min(data(:,10)) min(data(:,11)) min(data(:,12)) min(data(:,13)) min(data(:,14))];
        
        % Normalize input data
        sampleTest = (sampleTest-meanTrain)./stdTrain;
    
        % Eventually print values
        sampleTest(1:3); % xyz accelerations
        sampleTest(4:8); % fingers flexion

        predictions = model.predictFcn(sampleTest);

        data = data(ceil(size(data,1)/overlapFactor):end,:);
        counter = 0;
        sampleTest = [];
    end
end
clear device