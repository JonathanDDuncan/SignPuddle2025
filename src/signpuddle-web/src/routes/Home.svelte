<script>
  import { userStore } from '../stores/userStore';
</script>

<div class="home-page">
  <section class="hero">
    <div class="hero-content">
      <h1>SignPuddle 2</h1>
      <p class="tagline">Create, Edit, and Share Sign Language Dictionaries</p>
      
      <div class="hero-actions">
        {#if !$userStore.isAuthenticated}
          <button class="primary-btn" on:click={() => userStore.showLoginModal()}>
            Get Started
          </button>
        {:else}
          <a href="/my-puddles" class="primary-btn">My Puddles</a>
        {/if}
        <a href="/dictionary" class="secondary-btn">Browse Dictionaries</a>
      </div>
    </div>
  </section>

  <section class="features">
    <h2>Features</h2>
    
    <div class="features-grid">
      <div class="feature-card">
        <div class="feature-icon">✏️</div>
        <h3>Sign Maker</h3>
        <p>Create and edit individual signs using the SignWriting notation system.</p>
        <a href="/sign-maker" class="feature-link">Open Sign Maker</a>
      </div>
      
      <div class="feature-card">
        <div class="feature-icon">📝</div>
        <h3>Sign Text</h3>
        <p>Compose sign language texts by combining multiple signs and formatting them.</p>
        <a href="/sign-text" class="feature-link">Open Sign Text</a>
      </div>
      
      <div class="feature-card">
        <div class="feature-icon">📚</div>
        <h3>Dictionaries</h3>
        <p>Browse and search sign language dictionaries from around the world.</p>
        <a href="/dictionary" class="feature-link">Browse Dictionaries</a>
      </div>
    </div>
  </section>

  <section class="about">
    <h2>About SignWriting</h2>
    
    <div class="about-content">
      <div class="about-text">
        <p>
          SignWriting is a system of writing sign languages that visually represents the handshapes, 
          movements, and facial expressions of signers.
        </p>
        <p>
          It was invented in 1974 by Valerie Sutton and has been used to write more than 40 
          sign languages around the world.
        </p>
        <p>
          SignPuddle 2 provides tools for creating and sharing SignWriting dictionaries, 
          making sign languages accessible in written form.
        </p>
      </div>
      
      <div class="about-image">
        <img src="/images/signwriting-example.png" alt="SignWriting Example" />
      </div>
    </div>
  </section>
</div>

<style>
  .home-page {
    max-width: 1200px;
    margin: 0 auto;
    padding: 0 1rem;
  }
  
  /* Hero section */
  .hero {
    padding: 3rem 0;
    background-color: #f5f8ff;
    border-radius: 8px;
    margin: 2rem 0;
    text-align: center;
  }
  
  .hero h1 {
    font-size: 2.5rem;
    color: #1a3c6e;
    margin-bottom: 1rem;
  }
  
  .tagline {
    font-size: 1.2rem;
    color: #555;
    margin-bottom: 2rem;
  }
  
  .hero-actions {
    display: flex;
    justify-content: center;
    flex-wrap: wrap;
    gap: 1rem;
  }
  
  .primary-btn, .secondary-btn {
    padding: 0.8rem 1.5rem;
    border-radius: 4px;
    font-weight: bold;
    text-decoration: none;
    cursor: pointer;
  }
  
  .primary-btn {
    background-color: #1a3c6e;
    color: white;
    border: none;
  }
  
  .secondary-btn {
    background-color: white;
    color: #1a3c6e;
    border: 1px solid #1a3c6e;
  }
  
  /* Features section */
  .features {
    padding: 3rem 0;
  }
  
  .features h2 {
    text-align: center;
    color: #1a3c6e;
    margin-bottom: 2rem;
  }
  
  .features-grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
    gap: 2rem;
  }
  
  .feature-card {
    background-color: white;
    border-radius: 8px;
    padding: 2rem;
    box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
    text-align: center;
  }
  
  .feature-icon {
    font-size: 2.5rem;
    margin-bottom: 1rem;
  }
  
  .feature-card h3 {
    color: #1a3c6e;
    margin-bottom: 1rem;
  }
  
  .feature-link {
    display: inline-block;
    margin-top: 1rem;
    color: #1a3c6e;
    text-decoration: none;
    font-weight: bold;
  }
  
  .feature-link:hover {
    text-decoration: underline;
  }
  
  /* About section */
  .about {
    padding: 3rem 0;
  }
  
  .about h2 {
    text-align: center;
    color: #1a3c6e;
    margin-bottom: 2rem;
  }
  
  .about-content {
    display: flex;
    flex-wrap: wrap;
    align-items: center;
    gap: 2rem;
  }
  
  .about-text {
    flex: 1;
    min-width: 300px;
  }
  
  .about-image {
    flex: 1;
    min-width: 300px;
    text-align: center;
  }
  
  .about-image img {
    max-width: 100%;
    border-radius: 8px;
    box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
  }
</style>