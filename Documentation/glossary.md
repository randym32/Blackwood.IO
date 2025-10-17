# Glossary

This glossary defines terms and concepts used in the library documentation.

## A

**Application Name**
: The name of the current application, typically derived from the entry assembly name. Used by the `Application.Name` property to identify the running application.

## B

**Background Save**
: A file saving mechanism that prevents UI blocking.  Implemented by the `Util.SaveBackground` method.

## C

**Cache**
: An in-memory storage mechanism that provides fast access to frequently used data.  The Blackwood.IO cache uses a LRU (Least Recently Used) eviction policy.

**Cache Item**
: A single entry in the cache containing a key-value pair with metadata for tracking access order and expiration.

**Cache Key**
: The unique identifier used to store and retrieve items from the cache.

**Cache Value**
: The actual data stored in the cache associated with a specific key.

## D

**Doubly-Linked List**
: A data structure where each node contains references to both the next and previous nodes. Used in the cache implementation to maintain access order for LRU eviction.

## E

**Embedded Resources**
: Files that are compiled into the assembly and can be accessed at runtime without requiring separate file deployment.

**Entry Assembly**
: The assembly that contains the application's entry point (typically the Main method). Used to find resources associated with the application, and determine the application name, .

## F

**File Stream**
: A stream that provides read/write access to a file on disk.

**Folder Wrapper**
: An abstraction layer that provides a consistent interface for folder operations across different storage systems.

## H

**Hash Table**
: A data structure that provides fast key-value lookups using hash codes. Used internally by the cache for O(1) access time.

## L

**LRU (Least Recently Used)**
: A cache eviction policy that removes the least recently accessed items when the cache reaches its capacity limit.

## M

**Memory Cache**
: An in-memory storage system that keeps frequently accessed data in RAM for fast retrieval.

## P

**Path Operations**
: Utility functions for working with file and directory paths, including path resolution and manipulation.

## R

**Race Condition**
: A situation where multiple threads access shared resources simultaneously, potentially causing unpredictable behavior. Prevented using locks in background save operations.

## S

**Stack Trace**
: A representation of the call stack at a particular point in time, used as a fallback method to determine the application assembly.

**Substitute Variables**
: A text processing feature that replaces placeholder variables (in the format `{{variableName}}`) with actual values from a provided dictionary.

## T

**Tableau**
: A dictionary or collection of key-value pairs used for variable substitution in text processing.

**Temporary File**
: A file created during background save operations to ensure atomic writes and prevent data corruption.

**Thread-Safe**
: Code that can be safely executed by multiple threads simultaneously without causing race conditions or data corruption.

## V

**Variable Substitution**
: The process of replacing placeholder variables in text with actual values from a provided data source.

## W

**Write Background**
: A delegate type that defines the signature for background file writing operations.

## Z

**Zip Wrapper**
: An abstraction layer that provides a consistent interface for ZIP file operations.
