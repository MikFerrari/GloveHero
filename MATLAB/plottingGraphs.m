load('D:\Desktop\DMSR - Final Project\preprocessedData\2022_1_4_S3_N3.mat');
close all;
clc;
ax = figure;
spessore = 1.5;
time = time2num(dataPrep.Time);
plot(time, dataPrep.Finger1,'DisplayName','dataPrep.Finger1','LineWidth',spessore);hold on;
plot(time, dataPrep.Finger2,'DisplayName','dataPrep.Finger2','LineWidth',spessore);
plot(time, dataPrep.Finger3,'DisplayName','dataPrep.Finger3','LineWidth',spessore);
plot(time, dataPrep.Finger4,'DisplayName','dataPrep.Finger4','LineWidth',spessore);
plot(time, dataPrep.Finger5,'DisplayName','dataPrep.Finger5','LineWidth',spessore);hold off;
grid on
legend('F1','F2','F3','F4','F5','Location','north','Orientation','horizontal')
xlim([time(1) time(end)])
xlabel('Time [s]')
ylabel('Voltage [mV]')
title('Pinch 2 (Thumb-Index)')
dir = 'D:\Programmi\OneDrive - unibs.it\Universita\Anno_5\S1_Parigi\DMSR - Designing mechatronic systems for rehabilitation\Designin_Project_Exam\Presentation\';
exportgraphics(ax, strcat(dir,'P2_signals.png'))