function dataset = uniformData(dataset, NsamplesPerClass)

% dataset: table with features along the columns and the last column
%          containing labels.
% NsamplesPerClass: desired number of samples per label.

classes = {'P2','P3','P4','P5','F','S','N'};

N_of_samples = NaN(length(classes),1);

for i = 1:length(classes)
    
    dataThisClass = dataset((dataset.Label == classes{i}),:); % find rows find the specific dataset

    if isempty(dataThisClass) == 0
        L = NsamplesPerClass - size(dataThisClass,1);
    
        if L > 0
            for j = 1:L
                index = randi([1,size(dataThisClass,1)]);
                newdata = dataThisClass(index,:);
                dataset = [dataset; newdata];
            end

        elseif L < 0
            for j = 1:-L
                v = find(dataset.Label == classes{i});
                index = randi([1,length(v)]);
                dataset(v(index),:) = [];
            end
        end

    end
end

for i = 1:length(classes)
    N_of_samples(i) = sum(dataset.Label == classes{i});
end