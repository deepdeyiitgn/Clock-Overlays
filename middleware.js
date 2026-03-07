export default function middleware(request) {
  const url = new URL(request.url);
  
  // Rule 1: Agar path '/assets' se shuru ho raha hai
  // Toh isko bypass kar do (koi ched-chhad nahi, images apne original capital/small naam se load hongi)
  if (url.pathname.startsWith('/assets')) {
    return; 
  }
  
  // Rule 2: Baaki baache huye saare pages (jaise /About.html, /Features) ke liye
  // Agar URL mein koi bhi Capital letter hai, toh usko small letter (lowercase) kardo
  if (url.pathname !== url.pathname.toLowerCase()) {
    url.pathname = url.pathname.toLowerCase();
    return Response.redirect(url, 308); // User ko sahi small-letter wale URL par bhej do
  }
}

// Yeh Vercel ko batata hai ki middleware kin URL par chalana chahiye
export const config = {
  matcher: [
    // '/assets' ko chhor kar baaki saare URLs ko pakro
    '/((?!assets).*)'
  ]
};
