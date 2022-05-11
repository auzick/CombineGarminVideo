# Combine Garmin Video

## Usage
`path-to-program-folder\CombineGarminVideo.exe [path-to-source-video-folder] [-d] [-o path-to-output-folder]`

`[path-to-video-folder]` Optional; defaults to current folder<br/>
`-d, --delete`    (Default: false) Delete after processing.
`-o, --output`    (Default: current folder) folder where combined files are saved.

## General

My Garmin dashcam (the DashCam Mini, but presumably other Garmin dashcams as well), store video in one minute increments on the SD card.

This app will concatenate those one-minute videos into larger video files, by looking for series of files that are roughly within a minute of each other and assuming they form a seequence.

Using the `-d` switch will delete the source files after they have been combined.

The application will detect each sequence of videos and combine them into a file in the form `yyyyMMdd.HHmm.mpg` based on the start time of the first video in the ssequence.

This requires that you have [FFMpeg](https://ffmpeg.org/) installed. (`choco install ffmpeg`).
