import matplotlib.pyplot as plt
import numpy as np

data = []
f = open("..\\Output\\beatmap.csv", "r")

for point in f:
    data.append((point[0]))

count = 0
for i in data:
    if float(i) > float(0):
        count += 1

print(count)

time = np.linspace(
    0,  # start
    len(data) / (1024),
    num=len(data)
)

plt.figure(1)
plt.title("Sound Wave")

plt.xlabel("Time")

plt.plot(time, data, scalex=False)

plt.show()
