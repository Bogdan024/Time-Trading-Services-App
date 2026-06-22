import { Component, computed, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

type ChatMessage = {
  from: 'me' | 'them';
  text: string;
  time: string;
};

type Conversation = {
  id: string;
  name: string;
  topic: string;
  time: string;
  messages: ChatMessage[];
};

@Component({
  selector: 'app-messages',
  imports: [FormsModule],
  templateUrl: './messages.html',
  styleUrl: './messages.css',
})
export class Messages {
  protected draft = '';
  protected selectedId = signal('c1');

  protected conversations = signal<Conversation[]>([
    {
      id: 'c1', name: 'Ana', topic: 'Help move a sofa up two floors', time: '09:34',
      messages: [
        { from: 'them', text: 'Hi! Saw you might be free Saturday — could you give me a hand with the sofa?', time: '09:12' },
        { from: 'me', text: 'Sure, mornings work for me. Which floor was it again?', time: '09:30' },
        { from: 'them', text: 'Third floor, no elevator unfortunately 😅 I owe you 2 credits!', time: '09:34' },
      ],
    },
    {
      id: 'c2', name: 'Elena', topic: 'Set up a new laptop', time: 'Yesterday',
      messages: [
        { from: 'them', text: 'Thank you so much for offering to help with the laptop!', time: 'Yesterday' },
        { from: 'me', text: 'Happy to. Is Thursday evening okay? We can do it over a call.', time: 'Yesterday' },
      ],
    },
    {
      id: 'c3', name: 'Ioana', topic: 'Walk my dog this weekend', time: 'Mon',
      messages: [{ from: 'them', text: 'Max is already excited to meet you 🐕', time: 'Mon' }],
    },
    {
      id: 'c4', name: 'Mihai', topic: 'Help assemble a bookshelf', time: 'Jun 18',
      messages: [
        { from: 'them', text: 'Appreciate the assembly help last week — I left you a 5-star review!', time: 'Jun 18' },
        { from: 'me', text: 'Anytime, glad it held together 🙂', time: 'Jun 18' },
      ],
    },
  ]);

  protected selectedConv = computed(
    () => this.conversations().find(c => c.id === this.selectedId()) ?? this.conversations()[0],
  );

  protected initial(name: string) {
    return name.trim()[0]?.toUpperCase() ?? '?';
  }

  protected lastMessage(conv: Conversation) {
    return conv.messages[conv.messages.length - 1]?.text ?? '';
  }

  protected select(id: string) {
    this.selectedId.set(id);
  }

  protected send() {
    const text = this.draft.trim();
    if (!text) return;

    this.conversations.update(convs =>
      convs.map(c =>
        c.id === this.selectedId()
          ? { ...c, messages: [...c.messages, { from: 'me' as const, text, time: 'Now' }], time: 'Now' }
          : c,
      ),
    );
    this.draft = '';
  }
}
