﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitWindow {
    float frontendTime;
    float backendTime;

    private List<GuitarNoteHitKnowledge> m_noteQueue;

    public List<GuitarNoteHitKnowledge> noteKnowledgeQueue
    {
        get
        {
            return m_noteQueue;
        }
    }

    public GuitarNoteHitKnowledge oldestUnhitNote
    {
        get
        {
            // New notes inserted at the end
            for (int i = 0; i < noteKnowledgeQueue.Count; ++i)
            {
                if (!noteKnowledgeQueue[i].hasBeenHit)
                    return noteKnowledgeQueue[i];
            }
            return null;
        }
    }

    public HitWindow(float frontendTime, float backendTime)
    {
        m_noteQueue = new List<GuitarNoteHitKnowledge>();
        this.frontendTime = frontendTime;
        this.backendTime = backendTime;
    }

    public bool DetectEnter(Note note, float time)
    {
        if (note.time > time + frontendTime)
            return false;

        foreach (GuitarNoteHitKnowledge noteHitData in m_noteQueue)
        {
            if (note.position == noteHitData.note.position)
                return false;
        }

        m_noteQueue.Add(new GuitarNoteHitKnowledge(note));

        return true;
    }

    public List<GuitarNoteHitKnowledge> DetectExit(float time)
    {
        List<GuitarNoteHitKnowledge> elementsRemoved = new List<GuitarNoteHitKnowledge>();

        for (int i = m_noteQueue.Count - 1; i >= 0; --i)
        {
            GuitarNoteHitKnowledge noteKnowledge = m_noteQueue[i];
            Note note = noteKnowledge.note;
            Note nextNote = GetNextNote(i);

            bool fallenOutOfWindow = !IsWithinTimeWindow(note, nextNote, time);

            if (fallenOutOfWindow || noteKnowledge.shouldExitWindow)
            {
                elementsRemoved.Add(m_noteQueue[i]);
                m_noteQueue.RemoveAt(i);
            }
        }

        return elementsRemoved;
    }

    public int GetPosition(Note note)
    {
        for (int i = 0; i < m_noteQueue.Count; ++i)
        {
            GuitarNoteHitKnowledge noteHitData = m_noteQueue[i];
            if (note == noteHitData.note)
                return i;
        }

        return -1;
    }

    public GuitarNoteHitKnowledge Get(Note note, float time)
    {
        int index = GetPosition(note);

        if (index >= 0)
        {
            GuitarNoteHitKnowledge noteData = m_noteQueue[index];
            Note nextNote = GetNextNote(index);

            if (IsWithinTimeWindow(note, nextNote, time))
                return noteData;
        }

        return null;
    }

    public bool Contains(Note note, float time)
    {
        return Get(note, time) != null;
    }

    // Must have a note to test. Next note is optional.
    public bool IsWithinTimeWindow(Note note, Note nextNote, float time)
    {
        return !(note.time < time - backendTime                                                         // Out of window range
                || (nextNote != null && note.time <= time - (nextNote.time - note.time) / 2.0f));        // Half the distance to the next note
    }

    Note GetNextNote(int index)
    {
        return index < m_noteQueue.Count - 1 ? m_noteQueue[index + 1].note : null;
    }
}
