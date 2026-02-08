// Components/wiki.tsx
import { Helmet } from "react-helmet";
import React from "react";

/**
 * =================================================================================
 * FILE: wiki.tsx
 * PROJECT: QuickLink - Official Biographical Profile
 * AUTHOR: Deep Dey
 * ---------------------------------------------------------------------------------
 * DESCRIPTION:
 * This component renders a comprehensive, self-contained biographical page 
 * modeled after Wikipedia's "Vector" skin (2022 edition). It serves as the 
 * primary digital portfolio and academic log for the author.
 * * KEY FEATURES:
 * 1. ENCYCLOPEDIC CONTENT: Detailed sections on Early Life, Academics, 
 * Preparation Strategy, and Personal Interests.
 * 2. SEO ARCHITECTURE: Full implementation of Schema.org (JSON-LD), 
 * Open Graph, and Twitter Cards via React Helmet.
 * 3. TYPOGRAPHY SYSTEM: Uses 'Linux Libertine' for headings and 
 * standard sans-serif stacks for body text to emulate the Wiki reading experience.
 * 4. RESPONSIVE LAYOUT: A dual-column layout (Article + Infobox) that 
 * seamlessly stacks on mobile viewports using scoped CSS media queries.
 * 5. ASSET SECURITY: Custom wrapper implementation to prevent unauthorized 
 * interaction (e.g., right-click context menu) on embedded media widgets.
 * * DEPLOYMENT NOTES:
 * - Ensure 'react-helmet' is installed.
 * - Verify image paths in the CONFIG object match the /public directory.
 * - Update SITE_URL before production build.
 * =================================================================================
 */

// ---------------------------------------------------------
// 1. GLOBAL CONFIGURATION & DATA SOURCE
// ---------------------------------------------------------

// 🔴 CONFIGURATION: Update this URL to your actual production domain
const SITE_URL = "https://qlynk.vercel.app";

const CONFIG = {
  // --- Identity Assets ---
  portrait: "/wiki-images/Deep_Dey_New.png",
  quicklinkLogo: "/wiki-images/Quicklink-logo.png",
  signature: "/wiki-images/Deep_Dey_IITK_Image1.jpg",
  
  // --- External References & Context ---
  dharmanagarWiki: "https://en.wikipedia.org/wiki/Dharmanagar",
  
  // --- Social Handles & Project Links ---
  youtube: "https://www.youtube.com/channel/UCrh1Mx5CTTbbkgW5O6iS2Tw",
  github: "https://github.com/deepdeyiitgn",
  instagram: "https://www.instagram.com/deepdey.official/",
  allLinks: "https://qlynk.vercel.app/alllinks",
  
  // --- Embedded Widgets ---
  // Note: This widget is protected by a custom overlay wrapper in the render method.
  spotifyWidget: "https://6klabs.com/widget/spotify/1df4b229dbac1d186ee5c0fbf87d4582d7535095a5d3f18824559466bcd8fa7b",
  
  // --- Curated Playlists (Academic & Cultural) ---
  playlistStudyMain: "https://open.spotify.com/playlist/6KIXCU0MCMP86td8GmLgxj?si=4c2PhCL5QZaB3zXqzrtgEg",
  playlistFav: "https://open.spotify.com/playlist/148O9r4X3UuekPoPY3cs70?si=_tSNPDFyQF6I_3aCjfqmUw",
  playlistStudyAlt: "https://open.spotify.com/playlist/4WfyY6HWh2tAZpzcIBaqlc?si=PcPVThOfSfW44Z4zc3D1HA",
  playlistPersonal: "https://open.spotify.com/playlist/5TDJBbIoYxdv120nwkKeJa?si=MXLP1qCDSDmFDvWHrxPCKw",
  playlistDurgaPuja: "https://open.spotify.com/playlist/1COK7ewFoKyCCs5oc11EGE?si=UFEYLBCZR7KjncjEPk_RTw",
  
  // --- Spotify User Profiles ---
  profileMain: "https://open.spotify.com/user/31zvkcklbwgpasiuqqh6gwaflyre",
  profileAlt: "https://open.spotify.com/user/31efaf6ojqcdkqdxpz5zrnpyxjnu/",
  profilePremium: "https://open.spotify.com/user/cixlt2z71bbl1o1xlncgiwbvb",
};

// Helper: Generates absolute URLs for SEO meta tags
const getAbsoluteUrl = (path: string) => `${SITE_URL}${path}`;


// ---------------------------------------------------------
// 2. SCOPED STYLING SYSTEM (CSS-IN-JS)
// ---------------------------------------------------------
// This object defines the base styles for the component. 
// Note: Critical layout overrides for mobile are handled in the <style> block below.

const S = {
  // --- Page Wrapper ---
  // Isolates the Wiki component from the rest of the application's global styles.
  pageWrap: {
    minHeight: "100vh",
    // Custom gradient requested by author for personal branding within Wiki format
    background:
      "radial-gradient(circle at center, rgba(255,255,255,1) 0%, rgba(254,255,255,1) 25%, rgba(241,247,255,1) 40%, rgba(17,24,39,1) 100%)",
    backgroundColor: "#0f172a", // Fallback color
    // Font Stack: Prioritizes system sans-serifs for legibility
    fontFamily: 'sans-serif, Arial, "Helvetica Neue"', 
    color: "#202122", // Official Wikipedia dark grey (#202122)
    boxSizing: "border-box" as const,
    paddingBottom: 60,
    width: "100%",
    position: "relative" as const,
    zIndex: 1, 
  },
  
  // --- Layout Container ---
  // Centers content and manages the flex relationship between Article and Infobox
  container: {
    maxWidth: 1150,
    margin: "0 auto",
    display: "flex",
    gap: 24,
    alignItems: "flex-start",
    paddingTop: 20,
    paddingLeft: 24,
    paddingRight: 24,
  },

  // --- Main Article Content ---
  main: {
    flex: "1 1 0%",
    minWidth: 0, // Prevents flex item from overflowing
    background: "rgba(255,255,255,0.94)", // High opacity for readability
    padding: "32px 40px",
    borderRadius: 0, // Adheres to Wiki "Document" aesthetic
    boxShadow: "0 1px 4px rgba(0,0,0,0.08)",
    border: "1px solid #a2a9b1",
    lineHeight: 1.6,
    color: "#202122",
    fontSize: 15,
  },

  // --- Typography: Page Title ---
  // Uses Serif font to mimic Linux Libertine
  title: {
    fontFamily: "'Linux Libertine', 'Georgia', 'Times', serif",
    fontSize: "2.1rem",
    fontWeight: 400,
    margin: "0 0 0.25em 0",
    color: "#000000",
    borderBottom: "1px solid #a2a9b1",
    paddingBottom: "0.25em",
    lineHeight: 1.3,
    letterSpacing: "0.02em",
  },

  byline: {
    fontSize: 13,
    color: "#54595d",
    marginBottom: 24,
    fontStyle: "italic",
  },

  // --- Typography: Section Headers (H2) ---
  sectionTitle: {
    fontFamily: "'Linux Libertine', 'Georgia', 'Times', serif",
    fontSize: "1.6rem",
    fontWeight: 400,
    marginTop: "1.8em",
    marginBottom: "0.6em",
    borderBottom: "1px solid #a2a9b1",
    paddingBottom: "0.3em",
    color: "#000000",
    width: "100%",
    display: "flex",
    alignItems: "center",
  },

  // --- Typography: Sub-headers (H3) ---
  h3: {
    fontWeight: 700,
    fontSize: "1.2rem",
    margin: "1.4em 0 0.6em 0",
    color: "#000000",
    fontFamily: "sans-serif", // Wiki sub-headers are typically sans-serif
  },

  // --- Table of Contents (TOC) ---
  tocBox: {
    border: "1px solid #a2a9b1",
    padding: "12px 24px",
    marginBottom: 24,
    backgroundColor: "#f8f9fa",
    display: "inline-block",
    minWidth: 260,
    maxWidth: "100%",
    borderRadius: 2,
  },
  tocTitle: {
    textAlign: "center" as const,
    fontWeight: 700,
    marginBottom: 12,
    fontSize: 14,
  },
  tocList: {
    listStyleType: "none",
    margin: 0,
    paddingLeft: 0,
    fontSize: 13,
    lineHeight: 1.5,
  },
  tocItem: {
    marginBottom: 4,
  },

  // --- Text Elements ---
  p: {
    marginBottom: "0.8em",
    marginTop: "0.5em",
    textAlign: "justify" as const,
  },
  ul: {
    marginLeft: "1.6em",
    marginBottom: "0.8em",
    marginTop: "0.5em",
    listStyleType: "disc",
  },
  li: {
    marginBottom: "0.4em",
  },

  // --- Infobox (Right Sidebar) ---
  infoboxWrap: {
    width: 320,
    flex: "0 0 320px",
    maxWidth: 320,
    position: "relative" as const,
    fontSize: "90%",
    lineHeight: 1.5,
  },

  infobox: {
    backgroundColor: "#f8f9fa",
    border: "1px solid #a2a9b1",
    padding: "12px",
    borderRadius: 2,
    boxShadow: "0 2px 5px rgba(0,0,0,0.05)",
    width: "100%",
    boxSizing: "border-box" as const,
  },

  infoboxHeader: {
    backgroundColor: "#b0c4de", // Standard Wiki Header Blue
    padding: "6px",
    textAlign: "center" as const,
    fontWeight: "bold",
    marginBottom: "12px",
    fontSize: "1.1em",
  },

  infoboxImageContainer: {
    textAlign: "center" as const,
    padding: "4px 0 14px 0",
  },

  infoboxImage: {
    maxWidth: "100%",
    height: "auto",
    display: "inline-block",
    border: "1px solid #c8ccd1",
    backgroundColor: "#fff",
    padding: 3,
    boxSizing: "border-box" as const,
  },

  infoRowLabel: {
    fontWeight: 700,
    verticalAlign: "top" as const,
    paddingRight: "0.8em",
    width: "35%",
    color: "#202122",
  },

  signatureImg: {
    maxWidth: "140px",
    width: "100%",
    height: "auto",
    display: "block",
    margin: "0 auto",
    opacity: 0.85,
  },

  // --- Footer Section ---
  footerNote: {
    marginTop: 60,
    fontSize: 12,
    color: "#54595d",
    borderTop: "1px solid #a2a9b1",
    paddingTop: 16,
    fontStyle: "italic",
  },

  refList: {
    fontSize: 13,
    color: "#202122",
    paddingLeft: 32,
  },

  // --- Utility Buttons ---
  smallButtons: {
    display: "flex",
    gap: 8,
    flexWrap: "wrap" as const,
    marginTop: 12,
    justifyContent: "center",
  },
  smallBtn: {
    padding: "4px 10px",
    border: "1px solid #a2a9b1",
    backgroundColor: "#ffffff",
    color: "#0645ad",
    fontSize: 12,
    textDecoration: "none",
    borderRadius: 2,
    flex: "1 0 auto",
    textAlign: "center" as const,
    fontWeight: 600,
  },

  // --- Protected Media Container ---
  // Used for the Spotify widget to prevent interaction with the iframe source
  iframeContainer: {
    width: "100%",
    marginTop: 16,
    marginBottom: 16,
    borderRadius: 4,
    overflow: "hidden",
    background: "#000",
    boxShadow: "0 2px 8px rgba(0,0,0,0.1)",
    position: "relative" as const,
    border: "1px solid #a2a9b1",
  },

  clear: { clear: "both" as const },
};


// ---------------------------------------------------------
// 3. CONTENT GENERATION UTILITIES
// ---------------------------------------------------------

/**
 * expandText
 * Generates contextually relevant filler text to simulate the density 
 * of a real encyclopedia entry.
 * @param base - The core fact or statement.
 * @param times - The multiplier for expansion.
 */
function expandText(base: string, times = 2) {
  const expansions = [
    "The methodology emphasizes a systematic approach to problem-solving, prioritizing conceptual clarity over rote memorization.",
    "This period was marked by rigorous academic discipline and the development of structured daily routines.",
    "Reports indicate a consistent focus on iterative revision cycles, aligning with standard preparation strategies for national-level competitive examinations.",
    "Public documentation of these activities serves as a repository of study patterns and time-management techniques.",
    "The approach incorporates elements of the 'Feynman Technique' for concept reinforcement and active recall methods."
  ];
  let out = base;
  for (let i = 0; i < times; i++) {
    out += " " + expansions[i % expansions.length];
  }
  return out;
}


// ---------------------------------------------------------
// 4. MAIN COMPONENT EXPORT
// ---------------------------------------------------------

export default function WikiPage(): React.ReactElement {

  // --- Content Generators (Expanded for Length) ---
  
  const lead = expandText(
    "Deep Dey (born 21 October 2008) is an Indian student and aspiring engineer from Dharmanagar, Tripura. He is currently a candidate for the Joint Entrance Examination (JEE) scheduled for 2027, targeting admission into the Indian Institutes of Technology (IITs). He is noted for his digital documentation of academic preparation and the development of web utilities under the alias 'QuickLink'.", 
    2
  );

  const early = expandText(
    "Dey was born in Dharmanagar, North Tripura. He completed his primary and secondary education under the Tripura Board of Secondary Education (TBSE) curriculum at New Shishu Bihar Higher Secondary School. During this formative period, he demonstrated a strong aptitude for mathematics and general sciences, securing over 80% in his Madhyamik (10th Board) examinations.", 
    3
  );

  const academic = expandText(
    "Following his matriculation, Dey transitioned to the Central Board of Secondary Education (CBSE) curriculum at Golden Valley High School to better align his studies with the syllabus of national engineering entrance requirements. His academic records indicate a primary focus on Physics, Chemistry, and Mathematics (PCM).", 
    3
  );

  const prepStrategy = expandText(
    "Dey's preparation methodology for the 2027 Joint Entrance Examination involves a structured daily regimen heavily influenced by the 'Arjuna' and 'Manzil' batches of Physics Wallah. Key aspects include time-blocked study sessions, regular mock examinations, and detailed error analysis.", 
    3
  );

  const creation = expandText(
    "In parallel with his studies, Dey maintains a digital log of his academic journey. He records study sessions and occasional tutorials that illustrate problem-solving steps and time management approaches for peers. These materials are offered as examples of habit-based study rather than formal instruction.", 
    2
  );

  const projects = expandText(
    "As part of independent learning, Dey explores small web projects and demonstrator code to reinforce practical programming skills and application-level understanding. His flagship project, QuickLink, serves as a central hub for his digital presence.", 
    3
  );

  return (
    <>
      {/* ========================================================================
        SEO & META TAGS SECTION (React Helmet)
        ========================================================================
        This section injects critical metadata into the <head> for search engines
        and social media platforms.
      */}
      <Helmet>
        {/* Document Metadata */}
        <title>Deep Dey - Wiki</title>
        <meta name="title" content="Deep Dey - Wiki Style Profile" />
        <meta name="description" content="Deep Dey (born 2008) is an Indian student and JEE 2027 aspirant. Wiki-style profile documenting academic records, QuickLink project, and study resources." />
        <meta name="keywords" content="Deep Dey, Deep Dey Wiki, JEE 2027, IIT Aspirant, QuickLink, Dharmanagar Student, Deep Dey IIT Kanpur, IIT Kharagpur" />
        <meta name="author" content="Deep Dey" />
        
        {/* Viewport & Theme */}
        <meta name="viewport" content="width=device-width, initial-scale=1.0, maximum-scale=5.0" />
        <meta name="theme-color" content="#ffffff" />
        
        {/* Search Engine Directives */}
        <meta name="robots" content="index, follow" />
        <meta name="googlebot" content="index, follow" />

        {/* Open Graph (Facebook/Discord/WhatsApp) */}
        <meta property="og:type" content="article" />
        <meta property="og:title" content="Deep Dey - Wikipedia" />
        <meta property="og:description" content="Deep Dey: JEE 2027 Aspirant & Student Developer. View academic records, biography, and projects." />
        <meta property="og:url" content={`${SITE_URL}/wiki/Deep_Dey`} />
        <meta property="og:image" content={getAbsoluteUrl(CONFIG.portrait)} />
        <meta property="og:site_name" content="Wiki (Personal Edition)" />
        <meta property="article:section" content="Biography" />
        <meta property="article:tag" content="Student" />

        {/* Twitter Card */}
        <meta name="twitter:card" content="summary_large_image" />
        <meta name="twitter:site" content="@deepdey_official" />
        <meta name="twitter:title" content="Deep Dey - Wiki" />
        <meta name="twitter:description" content="Student profile of Deep Dey (JEE 2027). Target: IIT Kanpur/Kharagpur." />
        <meta name="twitter:image" content={getAbsoluteUrl(CONFIG.portrait)} />

        {/* JSON-LD Structured Data (Person Schema) */}
        <script type="application/ld+json">
          {JSON.stringify({
            "@context": "https://schema.org",
            "@type": "Person",
            "name": "Deep Dey",
            "url": `${SITE_URL}/wiki`,
            "image": getAbsoluteUrl(CONFIG.portrait),
            "jobTitle": "Student & JEE Aspirant",
            "birthDate": "2008-10-21",
            "birthPlace": {
              "@type": "Place",
              "name": "Dharmanagar, Tripura, India",
            },
            "nationality": "Indian",
            "knowsAbout": ["Physics", "Chemistry", "Mathematics", "Computer Science"],
            "alumniOf": [
              {
                "@type": "EducationalOrganization",
                "name": "New Shishu Bihar Higher Secondary School"
              }
            ],
            "sameAs": [
              CONFIG.youtube,
              CONFIG.github,
              CONFIG.allLinks,
              CONFIG.instagram
            ]
          })}
        </script>
      </Helmet>

      {/* ========================================================================
        SCOPED CSS INJECTION
        ========================================================================
        These styles are scoped to the '.wiki-scope' class to prevent leaking 
        into the main application's header/footer. 
      */}
      <style>{`
        /* Scoped Link Styling (Wiki Blue) */
        .wiki-scope a {
          color: #0645ad;
          text-decoration: none;
          font-family: sans-serif;
        }
        .wiki-scope a:visited { color: #0b0080; }
        .wiki-scope a:hover { text-decoration: underline; }
        .wiki-scope a:active { color: #faa700; }

        /* Superscript References */
        .wiki-scope sup {
          font-size: 0.75em;
          line-height: 0;
          position: relative;
          vertical-align: baseline;
          top: -0.5em;
        }
        
        /* Citation Styling */
        .wiki-scope .citation {
          color: #0645ad;
          cursor: pointer;
        }

        /* Mobile Optimization Media Queries */
        @media (max-width: 900px) {
          .wiki-container { 
            flex-direction: column-reverse !important; 
            padding: 10px !important;
            display: flex !important;
            width: 100% !important;
          }
          
          /* Infobox becomes a full-width card at the top/bottom */
          .wiki-infobox-wrap { 
            width: 100% !important; 
            max-width: 100% !important; 
            float: none !important;
            margin-bottom: 24px;
            border-bottom: 1px solid #a2a9b1;
          }
          
          .wiki-infobox { 
            width: 100% !important;
            float: none !important;
            margin: 0 !important;
            box-shadow: none !important;
            border: none !important;
          }
          
          /* Main content padding adjustment */
          .wiki-main { 
            padding-right: 0 !important;
            padding: 20px 16px !important;
            width: 100% !important;
            border: none !important;
          }
          
          .wiki-title {
            font-size: 1.8rem !important;
            margin-top: 8px;
          }
   

          /* Ensure infobox imagery scales on small screens */
          .wiki-infobox-img,
          .wiki-signature-img {
            width: 100% !important;
            max-width: 100% !important;
            height: auto !important;
            object-fit: contain !important;
          }
        }
      `}</style>

      {/* --- COMPONENT ROOT (Scoped Wrapper) --- */}
      <div className="wiki-scope" style={{width: '100%', overflowX: 'hidden'}}>
        <div style={S.pageWrap} className="wiki-wrap">
          <div style={S.container} className="wiki-container">
            
            {/* ==================================================================
              LEFT COLUMN: MAIN ARTICLE
              ==================================================================
            */}
            <article style={S.main} className="wiki-main" aria-labelledby="page-title">
              
              {/* Header */}
              <h1 id="page-title" style={S.title} className="wiki-title">
                Deep Dey
              </h1>
              <div style={S.byline}>
                From QuickLink, the free personal encyclopedia"
              </div>

              {/* Lead Section */}
              <p style={S.p}>
                <strong>Deep Dey</strong> (born 21 October 2008) is an Indian student from <a href={CONFIG.dharmanagarWiki} target="_blank" rel="noreferrer">Dharmanagar</a>, Tripura. He is currently an aspirant for the <a href="https://en.wikipedia.org/wiki/Joint_Entrance_Examination" target="_blank" rel="noreferrer">Joint Entrance Examination</a> (JEE) targeting the class of 2027. He is known for documenting his structured study practices and digital projects. {lead}
              </p>

              {/* Table of Contents (TOC) */}
              <div style={S.tocBox as React.CSSProperties}>
                <div style={S.tocTitle}>Contents</div>
                <ul style={S.tocList}>
                  <li style={S.tocItem}>1 <a href="#early-life">Early life and education</a></li>
                  <li style={S.tocItem}>2 <a href="#academic-record">Academic career</a></li>
                  <li style={S.tocItem}>3 <a href="#preparation">Examination preparation</a></li>
                  <li style={S.tocItem}>4 <a href="#content-creation">Content creation</a></li>
                  <li style={S.tocItem}>5 <a href="#personal-interests">Personal interests</a></li>
                  <li style={S.tocItem}><span style={{marginLeft:'1em'}}>5.1 Music and curation</span></li>
                  <li style={S.tocItem}><span style={{marginLeft:'1em'}}>5.2 Travel and media</span></li>
                  <li style={S.tocItem}>6 <a href="#projects">Projects</a></li>
                  <li style={S.tocItem}>7 <a href="#public-presence">Public presence</a></li>
                  <li style={S.tocItem}>8 <a href="#references">References</a></li>
                </ul>
              </div>

              {/* SECTION 1: Early Life */}
              <section id="early-life">
                <h2 style={S.sectionTitle}>Early life and education</h2>
                <p style={S.p}>{early}</p>
                <p style={S.p}>
                  He completed his early schooling under the Tripura Board of Secondary Education (TBSE) at <strong>New Shishu Bihar Higher Secondary School</strong>. On 16 November 2025, he was honored by the Vice Principal of his former school for securing over 80% in the Madhyamik examinations, a milestone that reinforced his academic discipline.
                </p>
                <p style={S.p}>
                  Later, he transitioned his academic focus toward the Central Board of Secondary Education (CBSE) curriculum at <strong>Golden Valley High School</strong> to better align with national-level engineering entrance requirements.
                </p>
              </section>

              {/* SECTION 2: Academic Record */}
              <section id="academic-record">
                <h2 style={S.sectionTitle}>Academic career</h2>
                <p style={S.p}>{academic}</p>
                <p style={S.p}>
                  His study logs reflect a rigorous focus on the core sciences. He maintains detailed summaries of subject-wise progress, prioritizing daily revision cycles and cumulative testing to track performance over time. His specific academic targets include admission to <strong>IIT Kharagpur</strong>, <strong>IIT Kanpur</strong>, or <strong>IIT Gandhinagar</strong>.
                </p>
              </section>

              {/* SECTION 3: Preparation Strategy */}
              <section id="preparation">
                <h2 style={S.sectionTitle}>Examination preparation</h2>
                <p style={S.p}>{prepStrategy}</p>
                <p style={S.p}>
                  His strategy is segmented by subject depth:
                </p>
                <ul style={S.ul}>
                  <li style={S.li}><strong>Foundation:</strong> Chapters requiring conceptual depth (e.g., Mechanics, Calculus) are covered via the 'Arjuna' batch.</li>
                  <li style={S.li}><strong>Coverage:</strong> Topics requiring broad overview or revision (e.g., Thermal Properties) are supplemented via the 'Manzil' marathon series.</li>
                  <li style={S.li}><strong>Routine:</strong> His daily schedule begins at 6:00 AM with Physics, followed by Chemistry theory, and concludes with Mathematics practice and revision in the evening.</li>
                </ul>
              </section>

              {/* SECTION 4: Content Creation */}
              <section id="content-creation">
                <h2 style={S.sectionTitle}>Content creation</h2>
                <p style={S.p}>{creation}</p>
                <p style={S.p}>
                  Originally operating under the handle 'Tarzan The Gamer' with Minecraft content, Dey rebranded to focus on educational vlogs. He adheres to a strict upload schedule (3-4 times per month) to ensure his primary focus remains on JEE preparation.
                </p>
              </section>

              {/* SECTION 5: Personal Interests (Music & Movies) */}
              <section id="personal-interests">
                <h2 style={S.sectionTitle}>Personal interests</h2>
                
                <h3 style={S.h3} id="music">Music and curation</h3>
                <p style={S.p}>
                  Dey curates music across multiple digital profiles. His listening habits are functional, often utilizing instrumental "Lofi" beats to aid concentration during long study hours, as well as regional music for cultural events like Durga Puja.<sup>[1]</sup>
                </p>
                
                {/* SECURITY: Context Menu Blocker 
                  Prevents users from Right-Clicking the Iframe to inspect source.
                */}
                <div 
                  style={S.iframeContainer} 
                  onContextMenu={(e) => {
                    e.preventDefault(); 
                    return false;
                  }}
                >
                  <iframe 
                    src={CONFIG.spotifyWidget} 
                    style={{ width: "100%", height: "160px", border: "none" }} 
                    title="Spotify Now Playing" 
                    loading="lazy"
                    allow="autoplay; clipboard-write; encrypted-media; fullscreen; picture-in-picture"
                  ></iframe>
                </div>
                <p style={{ fontSize: 13, fontStyle: "italic", color: "#54595d", marginTop: -8, marginBottom: 15 }}>
                  Above: Real-time "Now Playing" activity tracker via 6klabs.
                </p>

                {/* Secondary Spotify widget (same protected container) */}
                <div 
                  style={S.iframeContainer} 
                  onContextMenu={(e) => {
                    e.preventDefault(); 
                    return false;
                  }}
                >
                  <iframe 
                    src="https://6klabs.com/widget/spotify/3a527ea5812aee9245441bbee13e33cbff4c5b0a6c15595788db87d68e6d5e7b" 
                    style={{ width: "100%", height: "160px", border: "none" }} 
                    title="Spotify Now Playing (2)" 
                    loading="lazy"
                    allow="autoplay; clipboard-write; encrypted-media; fullscreen; picture-in-picture"
                  ></iframe>
                </div>
                <p style={{ fontSize: 13, fontStyle: "italic", color: "#54595d", marginTop: -8, marginBottom: 15 }}>
                  Above: Additional activity tracker widget via 6klabs.
                </p>

                <h3 style={S.h3}>Notable Playlists</h3>
                <ul style={S.ul}>
                  <li style={S.li}>
                    <strong>Primary Study Compilation:</strong> A comprehensive collection of over 400 tracks designed for long-duration focus blocks. 
                    <br />
                    <a href={CONFIG.playlistStudyMain} target="_blank" rel="noreferrer">View Playlist</a>
                  </li>
                  <li style={S.li}>
                    <strong>Secondary Focus List:</strong> An alternative study set for lighter revision sessions.
                    <br />
                    <a href={CONFIG.playlistStudyAlt} target="_blank" rel="noreferrer">View Playlist</a>
                  </li>
                  <li style={S.li}>
                    <strong>Cultural Selection:</strong> A dedicated curation for Durga Puja festivities.
                    <br />
                    <a href={CONFIG.playlistDurgaPuja} target="_blank" rel="noreferrer">View Playlist</a>
                  </li>
                  <li style={S.li}>
                    <strong>Personal Favorites:</strong> A non-academic collection reflecting personal musical taste.
                    <br />
                    <a href={CONFIG.playlistFav} target="_blank" rel="noreferrer">View Collection 1</a> &bull; <a href={CONFIG.playlistPersonal} target="_blank" rel="noreferrer">View Collection 2</a>
                  </li>
                </ul>

                <h3 style={S.h3}>Travel and media</h3>
                <p style={S.p}>
                  Dey has expressed a strong desire to visit London, United Kingdom, influenced by the film <i>Loveyatri</i>. His cinematic preferences also include <i>Satyaprem Ki Katha</i> and the <i>Bhool Bhulaiyaa</i> franchise.
                </p>
              </section>

              {/* SECTION 6: Projects */}
              <section id="projects">
                <h2 style={S.sectionTitle}>Projects and technical work</h2>
                <p style={S.p}>{projects}</p>
                <p style={S.p}>
                  He operates under the handle <i>QuickLink</i> for his web projects. These repositories serve as practical demonstrations of his coding skills, balancing his engineering preparation with technical application.<sup>[2]</sup>
                </p>
              </section>

              {/* SECTION 7: Public Presence */}
              <section id="public-presence">
                <h2 style={S.sectionTitle}>Public presence and channels</h2>
                <p style={S.p}>
                  Dey maintains a set of public profiles and a link hub used to publish study content
                  and project updates. These channels primarily consist of code-hosting accounts and
                  study-related uploads.
                </p>
                <ul style={S.ul}>
                  <li style={S.li}>YouTube — <a href={CONFIG.youtube} target="_blank" rel="noreferrer">{CONFIG.youtube}</a></li>
                  <li style={S.li}>GitHub — <a href={CONFIG.github} target="_blank" rel="noreferrer">{CONFIG.github}</a></li>
                  <li style={S.li}>All-links hub — <a href={CONFIG.allLinks} target="_blank" rel="noreferrer">{CONFIG.allLinks}</a></li>
                </ul>
              </section>

              <div style={S.clear} />

              {/* SECTION 8: References */}
              <section id="references">
                <h2 style={S.sectionTitle}>References</h2>
                <ol style={S.refList}>
                  <li id="cite_note-1"><b>^</b> <a href={CONFIG.spotifyWidget} rel="nofollow">Spotify Activity Widget</a>, 6klabs. Retrieved 2025.</li>
                  <li id="cite_note-2"><b>^</b> <a href={CONFIG.allLinks}>QuickLink Official Hub</a>. Retrieved 2025.</li>
                  <li id="cite_note-3"><b>^</b> <a href={CONFIG.dharmanagarWiki}>Dharmanagar Geography</a>, Wikipedia. Retrieved 2025.</li>
                  <li id="cite_note-4"><b>^</b> <a href={CONFIG.youtube}>YouTube Channel Statistics</a>, Deep Dey. Retrieved 2025.</li>
                </ol>
              </section>

              <div style={S.footerNote}>
                This page was last edited on 5 December 2025, at 19:52 (IST). <br />
                Text is available under the Creative Commons Attribution-ShareAlike License; additional terms may apply.
              </div>
            </article>

            {/* ==================================================================
              RIGHT COLUMN: INFOBOX
              ==================================================================
            */}
            <aside style={S.infoboxWrap} className="wiki-infobox-wrap" aria-label="Infobox column">
              <div style={S.infobox} className="wiki-infobox">
                
                <div style={{fontSize: 18, marginBottom: 8, textAlign: 'center', fontFamily: "'Linux Libertine', 'Georgia', serif", borderBottom: '1px solid #a2a9b1', paddingBottom: 6}}>
                  Deep Dey
                </div>

                <div style={S.infoboxImageContainer}>
                  <img
                    src={CONFIG.portrait}
                    alt="Deep Dey portrait"
                    style={S.infoboxImage}
                    className="wiki-infobox-img"
                    loading="lazy"
                  />
                  <div style={{fontSize: '11px', marginTop: '4px', lineHeight: '1.2'}}>
                    Dey in 2025
                  </div>
                </div>

                <div style={{ marginBottom: 12, color: "#111827", fontSize: 13, lineHeight: 1.4 }}>
                  <strong>Born</strong><br/>
                  21 October 2008 (age 17)<br />
                  <a href={CONFIG.dharmanagarWiki} style={{textDecoration:'none', color:'#0645ad'}}>Dharmanagar</a>, Tripura, India
                </div>

                <table style={{ width: "100%", fontSize: 13, borderCollapse: "collapse" }}>
                  <tbody>
                    <tr>
                      <td style={S.infoRowLabel as React.CSSProperties}>Nationality</td>
                      <td>Indian</td>
                    </tr>
                    <tr>
                      <td style={S.infoRowLabel as React.CSSProperties}>Occupation</td>
                      <td>Student</td>
                    </tr>
                    <tr>
                      <td style={S.infoRowLabel as React.CSSProperties}>Education</td>
                      <td>
                        New Shishu Bihar H.S. School (TBSE)<br />
                        Golden Valley High School (CBSE)
                      </td>
                    </tr>
                    <tr>
                      <td style={S.infoRowLabel as React.CSSProperties}>Family</td>
                      <td>
                        Biman Dey (Father)<br />
                        Jaya Dey (Mother)<br />
                        Puja Dey (Sister)<br />
                        Parul Rani Dey (Grandmother)
                      </td>
                    </tr>
                    <tr>
                      <td style={S.infoRowLabel as React.CSSProperties}>Targets</td>
                      <td>
                        <a href="https://www.iitkgp.ac.in/">IIT Kharagpur</a><br/>
                        <a href="https://www.iitk.ac.in/">IIT Kanpur</a><br/>
                        <a href="https://iitgn.ac.in/">IIT Gandhinagar</a>
                      </td>
                    </tr>
                  </tbody>
                </table>

                <div style={{ marginTop: 16 }}>
                  <div style={{ fontWeight: 700, marginBottom: 6 }}>Signature</div>
                  <img src={CONFIG.signature} alt="Signature" style={S.signatureImg} className="wiki-signature-img" loading="lazy" />
                </div>

                <div style={{ marginTop: 16, padding: "8px", background: "#ffffff", borderRadius: 2, border: "1px solid #c8ccd1" }}>
                  <div style={{ fontWeight: 700, fontSize: 11, color: "#54595d", textTransform: 'uppercase' }}>Favourite Quote</div>
                  <div style={{ marginTop: 4, fontStyle: "italic" as const, fontSize: 12 }}>
                    “✨ Remember: 100% effort + extra 1% = Dream Achieved”
                  </div>
                </div>

                <div style={{ marginTop: 16, borderTop: "1px solid #a2a9b1", paddingTop: 8, textAlign: 'center' }}>
                  <div style={{fontWeight: 700, fontSize: 12}}>External Links</div>
                  <div style={S.smallButtons}>
                    <a style={S.smallBtn} href={CONFIG.youtube} target="_blank" rel="noreferrer">YouTube</a>
                    <a style={S.smallBtn} href={CONFIG.github} target="_blank" rel="noreferrer">GitHub</a>
                    <a style={S.smallBtn} href={CONFIG.allLinks} target="_blank" rel="noreferrer">Website</a>
                  </div>
                </div>
              </div>
            </aside>
          </div>
        </div>

      </div>
    </>
  );
}
