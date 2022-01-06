clc; clear; close all;

%% ROUTINE FOR LABELING
% 1 - Load a specific acquisition
% 2 - Import the data from workspace into Signal Labeler
% 3 - Label data in the Signal Labeler
% 4 - Export to workspace
% 5 - Associate and extract labeled portions of the signal
% 6 - Save new table with labeled data


%% Import a specific file
idx_file = 15;
filename = strcat('2022_1_4_S2_N',num2str(idx_file));

load(strcat(".\preprocessedData\",filename));

%% Labelize signals trought Matlab Signal Labeler
% Open the Signal Labeler
% Import the label definition in "otherSavedFiles" folder
% Import dataPrep
% Labelize peaks
% Export labels (ls) in the Workspace

%% Create labels vector from Signal Labelizer output
waitforbuttonpress % Wait for user command to continue after ls export
labels = categorical(zeros(size(dataPrep,1),1));
output = ls.Labels.gesture{1,1};

for i = 1:size(output,1)

    ti = seconds(output.ROILimits(i,1));
    tf = seconds(output.ROILimits(i,2));
    L = output.Value(i);

    % Associate the label L to the corresponding dataPrep raws
    ind_i = find(abs(dataPrep.Time - ti)<seconds(1e-4));
    ind_f = find(abs(dataPrep.Time - tf)<seconds(1e-4));
    labels(ind_i:ind_f) = L;
end

% Define not-labeled portions of signal as class 'N'
labels(labels == '0') = categorical("N");

dataLabelled = [dataPrep table(labels)];
dataLabelled.Properties.VariableNames{end} = 'Label';

%% Save timetable with the labeled portion
directory = ".\labelledData_fullPeak\";
save(strcat(directory,names{idx_file}),'dataLabelled')