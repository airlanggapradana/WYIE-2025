using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class BookPage
{
    [SerializeField, TextArea(5, 10)] private string pageContent;
    [SerializeField] private Sprite pageImage; // Optional image to display on the page
    
    public string PageContent => pageContent;
    public Sprite PageImage => pageImage;
}

[CreateAssetMenu(fileName = "New Book", menuName = "Interactive Items/Book Data")]
public class BookData : ScriptableObject
{
    [SerializeField] private string bookTitle = "Untitled Book";
    [SerializeField] private string bookAuthor = "Unknown Author";
    [SerializeField] private Sprite bookCover; // Optional cover image
    [SerializeField] private List<BookPage> pages = new List<BookPage>();
    
    // Book identification
    [SerializeField] private string bookID; // Used for tracking which books have been read
    
    // Properties
    public string BookTitle => bookTitle;
    public string BookAuthor => bookAuthor;
    public Sprite BookCover => bookCover;
    public List<BookPage> Pages => pages;
    public string BookID => bookID;
    public int PageCount => pages.Count;
} 