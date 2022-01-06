%% Extract useful variables and features -> create datasetTrain & Test
clc; clear
Ntrials = 15;

%% Import datasets
%%{
dataset21 = table();
for i = 1:Ntrials
    filename = strcat('2022_1_3_S2_N',num2str(i));
    load(strcat(".\labelledData_fullPeak\",filename));
    dataML = [dataLabelled(:,1:3), dataLabelled(:,10:end)];
    dataML.Properties.VariableNames = {'xAcc','yAcc','zAcc','Finger1','Finger2','Finger3','Finger4','Finger5','Label'};
    
    % Extract features
    dataset21 = [dataset21; featuresExtractor(dataML)];

    % Save timetable
%     save(strcat(".\dataML\",filename),'dataML')

end
%}
%%{
dataset31 = table();
for i = 1:Ntrials
    filename = strcat('2022_1_4_S3_N',num2str(i));
    load(strcat(".\labelledData_fullPeak\",filename));
    dataML = [dataLabelled(:,1:3), dataLabelled(:,10:end)];
    dataML.Properties.VariableNames = {'xAcc','yAcc','zAcc','Finger1','Finger2','Finger3','Finger4','Finger5','Label'};
    
    % Extract features
    dataset31 = [dataset31; featuresExtractor(dataML)];

    % Save timetable
%     save(strcat(".\dataML\",filename),'dataML')

end
%}
%%{
dataset11 = table();
for i = 1:Ntrials
    filename = strcat('2022_1_4_S1_N',num2str(i));
    load(strcat(".\labelledData_fullPeak\",filename));
    dataML = [dataLabelled(:,1:3), dataLabelled(:,10:end)];
    dataML.Properties.VariableNames = {'xAcc','yAcc','zAcc','Finger1','Finger2','Finger3','Finger4','Finger5','Label'};
    
    % Extract features
    dataset11 = [dataset11; featuresExtractor(dataML)];

    % Save timetable
%     save(strcat(".\dataML\",filename),'dataML')

end
%}
%%{
dataset22 = table();
for i = 1:Ntrials
    filename = strcat('2022_1_4_S2_N',num2str(i));
    load(strcat(".\labelledData_fullPeak\",filename));
    dataML = [dataLabelled(:,1:3), dataLabelled(:,10:end)];
    dataML.Properties.VariableNames = {'xAcc','yAcc','zAcc','Finger1','Finger2','Finger3','Finger4','Finger5','Label'};
    
    % Extract features
    dataset22 = [dataset22; featuresExtractor(dataML)];

    % Save timetable
%     save(strcat(".\dataML\",filename),'dataML')

end
%}

%% Select training and testing datasets

dataTrain = [dataset31];
dataTest = dataset22;

% Remove 0 category
A = dataTrain.Label;
B = removecats(A,'0');
dataTrain.Label = B;
A = dataTest.Label;
B = removecats(A,'0');
dataTest.Label = B;
clear A B

%% Uniform data
% Uniform number of data
dataTrain = uniformData(dataTrain,200);
dataTest = uniformData(dataTest,100);

%% Features normalization
meanTrain = mean(table2array(dataTrain(:,1:end-1)));
stdTrain = std(table2array(dataTrain(:,1:end-1)));

temp = array2table((table2array(dataTrain(:,1:end-1))-meanTrain)./stdTrain);
temp.Properties.VariableNames = {'xRange'; 'yRange'; 'zRange'; 'Finger1Min'; 'Finger2Min'; 'Finger3Min'; 'Finger4Min'; 'Finger5Min'};
dataTrain = [temp table(dataTrain.Label)];
dataTrain.Properties.VariableNames{9} = 'Label';

temp = array2table((table2array(dataTest(:,1:end-1))-meanTrain)./stdTrain);
temp.Properties.VariableNames = {'xRange'; 'yRange'; 'zRange'; 'Finger1Min'; 'Finger2Min'; 'Finger3Min'; 'Finger4Min'; 'Finger5Min'};
dataTest= [temp table(dataTest.Label)];
dataTest.Properties.VariableNames{9} = 'Label';

%% Training & Testing KNN
clc;
[model, validationAccuracy] = KNNTrainer(dataTrain);
save('.\otherSavedFiles\KNN.mat','model')

% Compare Validation and Testing accuracies
validationAccuracy*100
predictions = model.predictFcn(dataTest);

output = (sum(predictions == dataTest.Label))/length(predictions)*100
ax = figure
plotconfusion(categorical(dataTest.Label'),categorical(predictions'))
