document.addEventListener('DOMContentLoaded', () => {
  // ==========================================
  // 1. Mobile Navigation Toggle
  // ==========================================
  const toggle = document.querySelector('.nav-toggle');
  const navLinks = document.querySelector('.nav-links');

  if (toggle && navLinks) {
    toggle.addEventListener('click', () => {
      navLinks.classList.toggle('open');
    });
  }

  // ==========================================
  // 2. Dynamic Copyright Year
  // ==========================================
  const yearTargets = document.querySelectorAll('.js-year');
  if (yearTargets.length) {
    const year = new Date().getFullYear();
    yearTargets.forEach((target) => {
      target.textContent = year;
    });
  }

  // ==========================================
  // 3. Premium 3D Mouse Tracking Tilt Effect
  // ==========================================
  // Ye selector index.html aur saare feature pages ki images ko catch karega
  const tiltElements = document.querySelectorAll('.screenshot-frame, .screenshot-frame-prep, .uptodown-screenshot, .uptodown-hero-img');

  tiltElements.forEach(el => {
    // Smooth transition setup
    el.style.transition = 'transform 0.2s cubic-bezier(0.25, 0.46, 0.45, 0.94)';
    el.style.willChange = 'transform';
    el.style.display = 'block'; // Ensures transform works perfectly
    
    el.addEventListener('mousemove', (e) => {
      const rect = el.getBoundingClientRect();
      
      // Calculate mouse position relative to the image
      const x = e.clientX - rect.left; 
      const y = e.clientY - rect.top;  
      
      // Calculate center of the image
      const centerX = rect.width / 2;
      const centerY = rect.height / 2;
      
      // Tilt logic (Max 8 degrees tilt for a premium look)
      const rotateX = ((y - centerY) / centerY) * -8; 
      const rotateY = ((x - centerX) / centerX) * 8;  
      
      // Apply 3D rotation and pop-out scale effect
      el.style.transform = `perspective(1000px) rotateX(${rotateX}deg) rotateY(${rotateY}deg) scale3d(1.03, 1.03, 1.03)`;
    });

    el.addEventListener('mouseleave', () => {
      // Jab mouse hatega, image smooth tarike se wapas normal ho jayegi
      el.style.transform = 'perspective(1000px) rotateX(0deg) rotateY(0deg) scale3d(1, 1, 1)';
    });
  });
});
