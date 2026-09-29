import { StudentForm } from './components/StudentForm';
import { StudentList } from './components/StudentList';

export default function App() {
  return (
    <main className="shell">
      <header className="masthead">
        <h1>Student registry</h1>
        <p>Add students and keep the list up to date.</p>
      </header>

      <div className="layout">
        <section className="panel form-panel" aria-labelledby="add-heading">
          <h2 id="add-heading">Add a student</h2>
          <StudentForm />
        </section>

        <section className="panel" aria-labelledby="list-heading">
          <h2 id="list-heading">Students</h2>
          <StudentList />
        </section>
      </div>
    </main>
  );
}
