# Combine Garmin Video

My Garmin dashcam (the DashCam Mini, but presumably other Garmin dashcams as well), store video in one minute increments on the SD card.

This app will concatenate those one-minute videos into larger video files, by looking for series of files that are roughly within a minute of each other and assuming they form a seequence.

To use it, copy all the files from one of the sd card directories (for example `DCIM\105UNSVD` for all unsaved video), and run `.\CombineGarminVideo.exe "<path to your folder>"`. The application will detect each sequence of videos and combine them into a file in the form `yyyyMMdd.HHmm.mpg` based on the start time of the first video in the ssequence.

This requires that you have [FFMpeg](https://ffmpeg.org/) installed. (`choco install ffmpeg`).
