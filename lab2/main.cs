using System;
using System.Collections.Generic;

class API
{
    private BaseDriver[] drivers;
    private DB db;

    public Book[] search(string query)
    {
        var raw = new List<RawBook>();
        foreach (var driver in drivers)
            raw.AddRange(driver.search(query));
        return compileBooks(raw.ToArray());
    }
    private Book[] compileBooks(RawBook[] rawBooks) { }
    public void download(BookSource source) { }
    public void removeBook(Book book)
    {
        foreach (var s in book.iterSources())
        {
            if (s.isDownloaded())
                deleteFiles(s);
        }
        db.removeBook(book);
    }
    public void deleteFiles(BookSource source) { }
}

abstract class BaseDriver
{
    private static Func<BaseDownloader> downloaderFactory;
    public string siteUrl;

    public RawBook[] search(string query) { }
    public void download(
        Book book,
        BookSource source,
        BaseDownloadProcessHandler processHandler
    )
    {
        var downloader = downloaderFactory();
        downloader.download();
    }
}
abstract class BaseDownloader
{
    private BookSource source;
    private BaseDownloadProcessHandler processHandler;
    private File[] files;

    public void download()
    {
        var totalSize = prepare();
        processHandler.init(DownloadStatus.downloading, totalSize);
        downloadFiles();
        processHandler.init(DownloadStatus.finishing);
        finish();
    }
    public void terminate() { }
    private abstract int prepare() { }
    private void downloadFiles()
    {
        foreach (var file in files)
        {
            downloadFile(file);
        }
    }
    private void downloadFile(File file) { }
    private void finish() { }

}
abstract class BaseDownloadProcessHandler
{
    private DownloadStatus status;
    private int totalSize;
    private int doneSize = 0;

    public void init(int totalSize) { }
    public void progress(int size) { }
    public void finish() { }
    public abstract void showProgress() { }
}
class File
{
    public int index;
    public string name;
    public string url;
    public float duration;
    public int size;
}
enum DownloadStatus
{
    waiting,
    preparing,
    downloading,
    finishing,
    finished,
    terminating,
    terminated
}

class DB
{
    private string dbPath;

    public void saveBook(Book book) { }
    public void removeBook(Book book) { }
    public Book[] getLibrary(
        int limit,
        int offset,
        string sortBy,
        string author,
        string series,
        bool favorite,
        string status
    )
    { }
}


class Book
{
    public string title;
    public string author;
    public string seriesName;
    public string numberInSeries;
    public string description;
    public string addingDate;
    public bool favorite;
    public TextBook[] textSources;
    public AudioBook[] audioSources;

    public void addTextSource(TextBook source) { }
    public void addAudioSource(AudioBook source) { }
    public BookSource[] iterSources() { }
}
class RawBook
{
    public string title;
    public string author;
    public string seriesName;
    public string numberInSeries;
    public string description;
    public BookSource source;
}
abstract class BookSource
{
    public string source;
    public string preview;
    public BookStatus status;

    public bool isDownloaded() { }
}
class TextBook : BookSource
{
    public string publication;
    public string fileUrl;
}
class AudioBook : BookSource
{
    public string narrator;
    public string duration;
    public ListeningProgress progress;
    public Chapter[] chapters;
}
class ListeningProgress
{
    public int chapterIndex = 0;
    public int time = 0;
}
class Chapter
{
    public string title;
    public string fileUrl;
    public int fileIndex;
    public int startTime;
    public int endTime;
}
enum BookStatus
{
    newBook,
    inProcess,
    finished
}
