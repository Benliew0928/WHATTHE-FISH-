(() => {
  'use strict';
  const $ = selector => document.querySelector(selector);
  const $$ = selector => [...document.querySelectorAll(selector)];
  const stage = $('#stage');
  const state = { scene: 'attack', camera: 1, session: 'Host', page: 'camera', guide: 'controls', kick: null, dash: null, joy: null, moving: false, power: 0, previewHold: false, cancelArmed: false, stamina: 100, exhausted: false, recoverAt: 0, tackleUntil: 0, last: performance.now(), event: 'Ready' };
  const iconNames = ['soccer-ball', 'sneaker-move', 'shield-chevron', 'sneaker', 'check', 'hand-swipe-left', 'target', 'flag-checkered', 'users-three'];
  const icons = {};
  const icon = name => `<span class="icon" data-icon="${name}">${icons[name] || ''}</span>`;
  function paintIcons() { $$('[data-icon]').forEach(node => { if (icons[node.dataset.icon]) node.innerHTML = icons[node.dataset.icon]; }); }
  Promise.all(iconNames.map(async name => { const response = await fetch(`icons/${name}.svg`); if (!response.ok) throw new Error(`Missing icon: ${name}`); icons[name] = await response.text(); })).then(paintIcons).catch(error => { console.error(error); });
  const defensive = () => ['defense', 'loose'].includes(state.scene);
  const ownsBall = () => !['defense', 'loose', 'teammate'].includes(state.scene);
  const holding = () => state.kick !== null || state.previewHold;
  let toastTimer;
  function toast(message) { clearTimeout(toastTimer); $('#toast').textContent = message; $('#toast').hidden = false; state.event = message; toastTimer = setTimeout(() => { $('#toast').hidden = true; }, 1800); }
  function capture(node, pointer) { try { node.setPointerCapture(pointer); } catch { /* Synthetic review events do not own native capture. */ } }
  function release(node, pointer) { if (pointer !== null && node.hasPointerCapture(pointer)) node.releasePointerCapture(pointer); }
  function resetJoy() { const old = state.joy; state.joy = null; state.moving = false; $('.stick').style.transform = ''; release($('#joystick'), old); }
  function endDash() { const old = state.dash; state.dash = null; state.recoverAt = performance.now() + 600; $('#dash').classList.remove('held'); release($('#dash'), old); render(); }
  function stopKick() { const old = state.kick?.pointer ?? null; state.kick = null; state.previewHold = false; state.cancelArmed = false; state.power = 0; $('#kick').classList.remove('held'); release($('#kick'), old); render(); }
  function clearInputs() { resetJoy(); endDash(); stopKick(); }
  function render() {
    const charging = holding() && !defensive();
    stage.classList.toggle('charging', charging);
    stage.classList.toggle('cancel-armed', charging && state.cancelArmed);
    stage.classList.toggle('defending', defensive());
    stage.classList.toggle('overtime', state.scene === 'overtime');
    stage.classList.toggle('exhausted', state.exhausted);
    $('#gesture').hidden = !charging;
    $('#primary-label').textContent = charging ? `${Math.round(state.power * 100)}%` : defensive() ? 'Pressure' : 'Kick';
    $('#primary-hint').textContent = charging ? 'POWER' : state.kick?.defense ? 'HOLDING' : state.scene === 'teammate' ? 'NO BALL' : state.scene === 'loose' ? 'NO CARRIER' : 'HOLD';
    $('#kick').disabled = ['teammate', 'loose'].includes(state.scene);
    $('#kick').setAttribute('aria-label', defensive() ? 'Pressure; hold to face the opposing carrier' : 'Kick; hold for power, release to kick');
    $('#kick .icon').dataset.icon = defensive() ? 'shield-chevron' : 'soccer-ball';
    $('#kick .icon').innerHTML = icons[$('#kick .icon').dataset.icon] || '';
    $('#tackle').hidden = !defensive();
    $('#jump').disabled = ownsBall();
    $('#jump-hint').textContent = ownsBall() ? 'OFF BALL' : 'TAP';
    $('#dash').disabled = (state.exhausted && state.dash === null) || holding();
    $('#dash-label').textContent = state.exhausted ? 'Rest' : 'Dash';
    $('#context-note').hidden = state.scene !== 'teammate';
    $('#clock').textContent = state.scene === 'overtime' ? '00:38' : '01:42';
    $('#phase').textContent = state.scene === 'overtime' ? 'GOLDEN GOAL' : 'MATCH';
    $('#score-a').textContent = state.scene === 'overtime' ? '1' : '2';
    $('.score').setAttribute('aria-label', state.scene === 'overtime' ? 'Team A 1, Team B 1. Golden goal, 38 seconds remaining.' : 'Team A 2, Team B 1. 1 minute 42 seconds remaining.');
    $('#release-cue').textContent = state.cancelArmed ? 'Release to cancel' : state.power >= 1 ? 'Full power · release' : 'Release to kick';
    $('#cancel-zone small').textContent = state.cancelArmed ? 'LIFT TO CANCEL' : 'SLIDE HERE';
    $('#drag-instruction').lastChild.textContent = state.cancelArmed ? 'Slide back to keep aiming' : 'Keep holding · slide to cancel';
    $('#power-arc').style.strokeDasharray = `${Math.min(1, state.power) * 75} 100`;
  }
  function selectScene(scene) {
    clearInputs();
    state.scene = scene;
    state.stamina = scene === 'exhausted' ? 10 : 100;
    state.exhausted = scene === 'exhausted';
    state.recoverAt = performance.now() + 600;
    state.tackleUntil = 0;
    if (scene === 'charge' || scene === 'cancel') { state.previewHold = true; state.power = .68; state.cancelArmed = scene === 'cancel'; }
    const notes = {
      charge: 'Power stays attached to Kick. It fills once, then holds — no green timing window.',
      cancel: 'Cancel is only armed here. Slide back to keep aiming; lift in this zone to cancel.',
      defense: 'Pressure takes the main slot. Hold to contain; tap Tackle for a separate challenge.',
      teammate: 'Kick stays in place but is unavailable. Jump and Dash stay within reach.',
      loose: 'No carrier to Pressure. Chase the loose ball or use Tackle; the main slot stays put.',
      exhausted: 'Rest replaces Dash until stamina recovers. Release the held finger before the next burst.',
      overtime: 'The same compact score strip becomes golden goal. The next goal wins.',
      attack: 'Hold Kick, slide to Cancel, then slide back before releasing.'
    };
    $('#review-note').innerHTML = `<b>${scene === 'attack' ? 'Try it:' : 'Review:'}</b> ${notes[scene]}`;
    $('#toast').hidden = true;
    render();
    paintStamina();
  }
  $('#preview').onchange = event => selectScene(event.target.value);
  function cancelContains(event) {
    const rect = $('#cancel-zone').getBoundingClientRect();
    const tolerance = 8 * (stage.clientHeight / 450);
    return event.clientX >= rect.left - tolerance && event.clientX <= rect.right + tolerance && event.clientY >= rect.top - tolerance && event.clientY <= rect.bottom + tolerance;
  }
  $('#kick').onpointerdown = event => {
    if (event.button !== 0 || state.kick || $('#kick').disabled) return;
    event.preventDefault();
    state.previewHold = false;
    endDash();
    state.kick = { pointer: event.pointerId, start: performance.now(), defense: defensive() };
    state.power = 0; state.cancelArmed = false;
    capture($('#kick'), event.pointerId);
    $('#kick').classList.add('held');
    if (defensive()) { $('#primary-hint').textContent = 'HOLDING'; toast('Pressure · face the carrier'); }
    render();
  };
  $('#kick').onpointermove = event => {
    if (state.kick?.pointer !== event.pointerId || state.kick.defense) return;
    state.cancelArmed = cancelContains(event); render();
  };
  $('#kick').onpointerup = event => {
    if (state.kick?.pointer !== event.pointerId) return;
    const pressure = state.kick.defense;
    const cancelled = !pressure && cancelContains(event);
    const power = state.power;
    $('#kick').classList.remove('held');
    stopKick();
    if (pressure) toast('Pressure released');
    else toast(cancelled ? 'Kick cancelled · ball kept' : power < .25 ? 'Light kick released' : `Kick released · ${Math.round(power * 100)}% power`);
  };
  $('#kick').onpointercancel = event => { if (event.pointerId === state.kick?.pointer) { $('#kick').classList.remove('held'); stopKick(); } };
  $('#kick').onlostpointercapture = event => { if (event.pointerId === state.kick?.pointer) { $('#kick').classList.remove('held'); stopKick(); } };
  $('#dash').onpointerdown = event => {
    if (event.button !== 0 || state.dash !== null || state.exhausted || holding()) return;
    event.preventDefault(); state.dash = event.pointerId; capture($('#dash'), event.pointerId); $('#dash').classList.add('held');
  };
  $('#dash').onpointerup = event => { if (event.pointerId === state.dash) endDash(); };
  $('#dash').onpointercancel = $('#dash').onpointerup;
  $('#dash').onlostpointercapture = $('#dash').onpointerup;
  $('#dash').onpointermove = event => {
    if (event.pointerId !== state.dash) return;
    const r = $('#dash').getBoundingClientRect();
    if (event.clientX < r.left || event.clientX > r.right || event.clientY < r.top || event.clientY > r.bottom) endDash();
  };
  $('#jump').onclick = () => { stopKick(); endDash(); toast('Jump'); };
  $('#tackle').onclick = () => { if (state.tackleUntil > performance.now()) return; stopKick(); endDash(); state.tackleUntil = performance.now() + 4000; toast('Tackle · recovering'); };
  $('#joystick').onpointerdown = event => { if (event.button !== 0 || state.joy !== null) return; event.preventDefault(); state.joy = event.pointerId; capture($('#joystick'), event.pointerId); moveStick(event); };
  function moveStick(event) {
    if (event.pointerId !== state.joy) return;
    const r = $('#joystick').getBoundingClientRect(), scale = stage.clientHeight / 450;
    const dx = (event.clientX - r.left - r.width / 2) / scale, dy = (event.clientY - r.top - r.height / 2) / scale;
    const magnitude = Math.hypot(dx, dy), clamp = Math.min(1, 25 / Math.max(1, magnitude));
    $('.stick').style.transform = `translate(${dx * clamp}px, ${dy * clamp}px)`;
    state.moving = magnitude > 7;
  }
  $('#joystick').onpointermove = moveStick;
  $('#joystick').onpointerup = event => { if (event.pointerId === state.joy) resetJoy(); };
  $('#joystick').onpointercancel = $('#joystick').onpointerup;
  $('#joystick').onlostpointercapture = $('#joystick').onpointerup;
  function paintStamina() {
    $('#stamina-fill').style.width = `${state.stamina}%`;
    $('.stamina').setAttribute('aria-valuenow', String(Math.round(state.stamina)));
    $('#stamina-caption').textContent = state.exhausted ? 'RECOVERING' : state.dash !== null ? state.moving ? 'DASHING' : 'MOVE TO DASH' : 'HOLD TO DASH';
  }
  function step(now) {
    const delta = Math.min(.05, (now - state.last) / 1000); state.last = now;
    if (state.kick && !state.kick.defense) { state.power = Math.min(1, (now - state.kick.start) / 1000); render(); }
    if (state.dash !== null && state.moving && !state.exhausted) {
      state.stamina = Math.max(0, state.stamina - 32 * delta); state.recoverAt = now + 600;
      if (state.stamina <= 0) { state.exhausted = true; render(); }
    } else if (now >= state.recoverAt && state.scene !== 'exhausted') {
      state.stamina = Math.min(100, state.stamina + 15 * delta);
      if (state.exhausted && state.dash === null && state.stamina >= 25) { state.exhausted = false; render(); }
    }
    const cooldown = Math.max(0, (state.tackleUntil - now) / 1000);
    $('#tackle').disabled = cooldown > 0;
    $('#tackle-hint').textContent = cooldown > 0 ? `${cooldown.toFixed(1)}s` : 'TAP';
    paintStamina(); requestAnimationFrame(step);
  }
  const cards = {
    controls: `<div class="guide-grid"><article class="guide-item"><img src="../BasketballUI/icons/move.svg" alt=""><div><h3>Move & look</h3><p>Left stick to move; push fully to run. Drag right to look.</p></div></article><article class="guide-item">${icon('soccer-ball')}<div><h3>Kick with control</h3><p>Tap for a light kick. Hold for power; release toward your target.</p></div></article><article class="guide-item">${icon('shield-chevron')}<div><h3>Defend</h3><p>Hold Pressure to face a carrier. Tap Tackle to challenge.</p></div></article><article class="guide-item"><img src="../BasketballUI/icons/jump.svg" alt=""><div><h3>Play off the ball</h3><p>Jump without the ball. Hold Dash while moving for a burst.</p></div></article></div><p class="guide-note"><b>Changed your mind?</b> Slide into Cancel and lift. Slide back to keep aiming.</p>`,
    rules: `<div class="guide-grid"><article class="guide-item">${icon('users-three')}<div><h3>Small teams, quick games</h3><p>1v1 to 5v5. The host starts with an even roster. Pick a team before kickoff.</p></div></article><article class="guide-item">${icon('flag-checkered')}<div><h3>Three minutes to win</h3><p>Each goal is one point. A tie goes to 60 seconds of golden goal, then a draw.</p></div></article><article class="guide-item">${icon('soccer-ball')}<div><h3>Find the ball</h3><p>Approach a slow free ball from the front to control it. Dribble, turn, then kick.</p></div></article><article class="guide-item">${icon('target')}<div><h3>Keep the match moving</h3><p>The boundary keeps the ball in play. A goal resets play for a three-second kickoff.</p></div></article></div><p class="guide-note">Exploration is free practice without scoring. Football currently has no collectible props.</p>`,
    energy: `<div class="energy-title">${icon('sneaker-move')}<div><h3>Save a burst for the break</h3><p>Dash is extra speed. Normal running does not spend stamina.</p></div></div><div class="energy-bar"><span></span></div><div class="energy-notes"><div><b>Hold while moving</b><p>A full bar lasts about three seconds. Let go to recover; standing still costs nothing.</p></div><div><b>Empty? Release and rest</b><p>Lift your finger. Dash becomes ready after the bar recovers to a quarter full.</p></div></div><p class="guide-note">Charging a kick ends Dash. Pressure lets you face a nearby carrier; it does not automatically win the ball.</p>`
  };
  function showPage(page) {
    state.page = page;
    $$('[data-page]').forEach(button => button.setAttribute('aria-pressed', String(button.dataset.page === page)));
    $('#camera-page').hidden = page !== 'camera'; $('#guide-page').hidden = page !== 'guide';
    if (page === 'guide') showGuide(state.guide);
  }
  function showGuide(guide) {
    state.guide = guide;
    $$('[data-guide]').forEach(button => { const selected = button.dataset.guide === guide; button.setAttribute('aria-selected', String(selected)); button.tabIndex = selected ? 0 : -1; button.id = `tab-${button.dataset.guide}`; });
    $('#guide-content').setAttribute('aria-labelledby', `tab-${guide}`);
    $('#guide-content').innerHTML = cards[guide]; paintIcons();
  }
  $$('[data-page]').forEach(button => { button.onclick = () => showPage(button.dataset.page); });
  $$('[data-guide]').forEach(button => { button.onclick = () => showGuide(button.dataset.guide); button.onkeydown = event => { if (!['ArrowLeft', 'ArrowRight'].includes(event.key)) return; event.preventDefault(); const tabs = $$('[data-guide]'), offset = event.key === 'ArrowRight' ? 1 : -1; const next = tabs[(tabs.indexOf(button) + offset + tabs.length) % tabs.length]; showGuide(next.dataset.guide); next.focus(); }; });
  $$('[data-camera]').forEach(button => { button.onclick = () => {
    state.camera = Number(button.dataset.camera);
    $$('[data-camera]').forEach(tile => tile.setAttribute('aria-pressed', String(Number(tile.dataset.camera) === state.camera)));
    $('#camera-detail').textContent = [
      'Look through your player’s eyes. Drag on the open right side to look around.',
      'Drag on the open right side to look around. Your left thumb stays on movement.',
      'A wider, ball-following view. Drag to pan your view; your player’s aim stays independent.'
    ][state.camera];
    $('#review-note').textContent = `${['Eye level', 'Follow', 'Stadium'][state.camera]} selected. This prototype previews the setting; it does not move the 3D camera.`;
  }; });
  function setReviewInert(value) { $('.review-options').inert = value; $('.review-footer label').inert = value; }
  function openSettings() {
    clearInputs(); $('#toast').hidden = true;
    $('#overlay').hidden = false; $('#settings-panel').hidden = false; $('#confirm-panel').hidden = true;
    $('#game-ui').inert = true; setReviewInert(true); showPage(state.page); $('#close-settings').focus({ preventScroll: true });
  }
  function closeSettings() { $('#overlay').hidden = true; $('#game-ui').inert = false; setReviewInert(false); $('#settings').focus({ preventScroll: true }); }
  $('#settings').onclick = openSettings;
  $('#close-settings').onclick = $('#resume').onclick = closeSettings;
  $('#session').onchange = event => { state.session = event.target.value; };
  $('#leave').onclick = () => {
    $('#settings-panel').hidden = true; $('#confirm-panel').hidden = false;
    const copy = {
      Host: ['Everyone returns to the room. The current match ends.', 'Return to room'],
      Guest: ['You leave the room. The other players can continue.', 'Leave room'],
      Offline: ['Return to sport selection. The current match ends.', 'Leave pitch']
    }[state.session];
    $('#leave-copy').textContent = copy[0]; $('#confirm-leave').textContent = copy[1]; $('#stay').focus({ preventScroll: true });
  };
  $('#stay').onclick = () => { $('#confirm-panel').hidden = true; $('#settings-panel').hidden = false; $('#leave').focus({ preventScroll: true }); };
  $('#confirm-leave').onclick = () => {
    $('#overlay').hidden = true; $('#end-screen').hidden = false;
    $('#end-screen h2').textContent = state.session === 'Host' ? 'Back in the room' : 'Back to sport selection';
    $('#reset').focus({ preventScroll: true });
  };
  $('#reset').onclick = () => { $('#end-screen').hidden = true; $('#game-ui').inert = false; setReviewInert(false); $('#preview').value = 'attack'; selectScene('attack'); $('#settings').focus({ preventScroll: true }); };
  document.addEventListener('keydown', event => {
    if (event.key === 'Escape') { if (!$('#confirm-panel').hidden && !$('#overlay').hidden) $('#stay').click(); else if (!$('#overlay').hidden) closeSettings(); else clearInputs(); }
    if (event.key !== 'Tab') return;
    const modal = !$('#end-screen').hidden ? $('#end-screen') : !$('#overlay').hidden ? $('#confirm-panel').hidden ? $('#settings-panel') : $('#confirm-panel') : null;
    if (!modal) return;
    const items = [...modal.querySelectorAll('button:not([disabled])')].filter(node => node.getClientRects().length && node.tabIndex >= 0);
    if (event.shiftKey && document.activeElement === items[0]) { event.preventDefault(); items.at(-1).focus(); }
    else if (!event.shiftKey && document.activeElement === items.at(-1)) { event.preventDefault(); items[0].focus(); }
  });
  function resize() {
    if (state.kick || state.dash !== null || state.joy !== null) clearInputs();
    stage.style.setProperty('--scale', stage.clientHeight / 450);
  }
  $('#screen-shape').onchange = event => { stage.style.aspectRatio = event.target.value.replace(':', '/'); resize(); };
  window.addEventListener('blur', clearInputs);
  document.addEventListener('visibilitychange', () => { if (document.hidden) clearInputs(); });
  new ResizeObserver(resize).observe(stage);
  resize(); render(); requestAnimationFrame(step);
})();
