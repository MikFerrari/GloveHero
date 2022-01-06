function features = featuresExtractor(data)
% data: timetable -> the last column is a categorical array containing the
% labels.

% % Normalize data
% data = [normalize(data(:,1:end-1)), table(data.Label)];
% data.Properties.VariableNames{9} = 'Label';

classes = {'P2','P3','P4','P5','F','S','N'};
features = table();

for k = 1:length(classes)
    temp = data.Label == classes{k};
    
    indexIn = [];
    indexFin = [];
    
    for i = 1:length(temp)
        if temp(i) == 1
            if i == 1
                indexIn = [indexIn;i];
            elseif i == length(temp)
                indexFin = [indexFin;i];
            elseif  temp(i-1) == 0 
                indexIn = [indexIn;i];
            elseif temp(i+1) == 0
                indexFin = [indexFin;i];
            end
        end
    end
    
    featTemp = [];
    for i = 1:size(indexIn,1)
        featTemp(i,:) = [mean(data.xAcc(indexIn(i):indexFin(i)))...
                          mean(data.yAcc(indexIn(i):indexFin(i)))...
                          mean(data.zAcc(indexIn(i):indexFin(i)))...
                          min(data.Finger1(indexIn(i):indexFin(i)))...
                          min(data.Finger2(indexIn(i):indexFin(i)))...
                          min(data.Finger3(indexIn(i):indexFin(i)))...
                          min(data.Finger4(indexIn(i):indexFin(i)))...
                          min(data.Finger5(indexIn(i):indexFin(i)))];
    end
    
    if isempty(indexIn) == 0 && isempty(indexFin) == 0 
        featTemp = [array2table(featTemp) table(data.Label(indexIn))];
        featTemp.Properties.VariableNames = {'xRange'; 'yRange'; 'zRange'; 'Finger1Min'; 'Finger2Min'; 'Finger3Min'; 'Finger4Min'; 'Finger5Min';'Label'};
        features = [features; featTemp];
    end
end