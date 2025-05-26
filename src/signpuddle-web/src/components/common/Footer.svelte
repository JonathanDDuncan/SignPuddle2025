<script>
  const currentYear = new Date().getFullYear();
</script>

<footer>
  <div class="footer-content">
    <div class="footer-section">
      <h4>SignPuddle 2</h4>
      <p>A free online tool for creating and sharing SignWriting dictionaries.</p>
    </div>
    
    <div class="footer-section">
      <h4>Resources</h4>
      <ul>
        <li><a href="https://www.signwriting.org/" target="_blank">SignWriting.org</a></li>
        <li><a href="https://www.signbank.org/" target="_blank">SignBank</a></li>
        <li><a href="/help">Help Documentation</a></li>
      </ul>
    </div>
    
    <div class="footer-section">
      <h4>Contact</h4>
      <ul>
        <li><a href="mailto:support@signpuddle.org">support@signpuddle.org</a></li>
        <li><a href="/feedback">Feedback</a></li>
      </ul>
    </div>
  </div>
  
  <div class="copyright">
    <p>&copy; {currentYear} SignPuddle 2. All rights reserved.</p>
  </div>
</footer>

<style>
  footer {
    background-color: #1a3c6e;
    color: white;
    padding: 2rem 1rem 1rem;
    margin-top: 2rem;
  }
  
  .footer-content {
    display: flex;
    flex-wrap: wrap;
    justify-content: space-around;
    gap: 2rem;
    max-width: 1200px;
    margin: 0 auto;
  }
  
  .footer-section {
    flex: 1;
    min-width: 200px;
  }
  
  h4 {
    margin-top: 0;
    margin-bottom: 1rem;
    font-size: 1.1rem;
  }
  
  ul {
    list-style: none;
    padding: 0;
    margin: 0;
  }
  
  li {
    margin-bottom: 0.5rem;
  }
  
  a {
    color: #bbd4ff;
    text-decoration: none;
  }
  
  a:hover {
    text-decoration: underline;
  }
  
  .copyright {
    margin-top: 2rem;
    text-align: center;
    font-size: 0.9rem;
    color: #bbd4ff;
    padding-top: 1rem;
    border-top: 1px solid rgba(255, 255, 255, 0.1);
  }
</style>