(() => {
  'use strict';
  const $ = id => document.getElementById(id);
  const stickers = [
    {id:'ggwp',name:'GGWP',meaning:'Good game, well played'},
    {id:'laugh',name:'Laugh',meaning:'That was hilarious'},
    {id:'cry',name:'Cry',meaning:'So close…'},
    {id:'angry',name:'Angry',meaning:'A little grumpy'},
    {id:'thanks',name:'Thanks',meaning:'Love the teamwork'},
    {id:'oops',name:'Oops',meaning:'My bad!'},
    {id:'wow',name:'Wow',meaning:'What a play'},
    {id:'lets-go',name:"Let’s go!",meaning:'Ready when you are'}
  ];
  const names = {lobby:'Room lobby',football:'Football',basketball:'Basketball'};
  const initial = [
    {kind:'context',text:'Room opened · 13:31'},
    {who:'Ben',kind:'text',text:'Ready for a game?',time:'13:31',scene:'lobby'},
    {who:'Guest',kind:'sticker',sticker:'lets-go',time:'13:31',scene:'lobby'},
    {kind:'context',text:'Travelled together → Football'},
    {who:'Ben',kind:'text',text:"Nice pass! I've got you.",time:'13:32',scene:'football'},
    {who:'BOT 3',kind:'sticker',sticker:'ggwp',time:'13:32',scene:'football'}
  ];
  let history = initial.slice(), scene = 'football', unread = 2, lastSend = 0, toastTimer, sendTimer;
  let quiet = false;
  const bubbleTimers = new Map();
  const storageKey = 'wtf-chat-review-v2:' + location.pathname;
  const params = new URLSearchParams(location.search);
  if (params.get('view') === 'stickers') document.body.classList.add('sticker-sheet-only');
  if (params.get('view') === 'game') document.body.classList.add('game-only');
  const make = (tag, cls, text) => { const node = document.createElement(tag); if(cls) node.className=cls; if(text!==undefined) node.textContent=text; return node; };
  function imageFor(id) {const def=stickers.find(s=>s.id===id);const image=make('img');image.src='stickers/'+id+'.png';image.alt=(def?.name||id)+' sticker';return image;}
  function save() { try {sessionStorage.setItem(storageKey,JSON.stringify({history,scene,unread,quiet}));} catch {} }
  try { const saved=JSON.parse(sessionStorage.getItem(storageKey));if(saved && Array.isArray(saved.history) && names[saved.scene]){history=saved.history.filter(m=>m && ['context','text','sticker'].includes(m.kind));scene=saved.scene;unread=Math.max(0,Number(saved.unread)||0);quiet=!!saved.quiet;} } catch {}
  function renderHistory(){
    const root=$('history');root.replaceChildren();
    history.forEach(m=>{
      if(m.kind==='context'){root.append(make('div','context-divider',m.text));return;}
      const entry=make('article','entry'), meta=make('div','entry-meta');
      meta.append(make('strong','',m.who+(m.who==='Ben'?' · You':'')),make('em','',names[m.scene]||'Room'),make('time','',m.time));entry.append(meta);
      entry.append(m.kind==='sticker'?imageFor(m.sticker):make('div','entry-text',m.text));root.append(entry);
    });root.scrollTop=root.scrollHeight;
  }
  function updateUnread(){ $('unread').textContent=unread>99?'99+':String(unread);$('unread').hidden=unread===0; }
  function clearBubbles(){ bubbleTimers.forEach(timer=>clearTimeout(timer));bubbleTimers.clear();$('bubbles').replaceChildren(); }
  function bubble(message, held=false){
    if(quiet || scene==='lobby') return;
    const who=message.who==='Ben'?'self':'peer';
    const old=$('bubble-'+who); if(old)old.remove();clearTimeout(bubbleTimers.get(who));
    const el=make('div','bubble '+(message.kind==='sticker'?'sticker':'text'));el.id='bubble-'+who;
    const anchors={football:{self:[800,596],peer:[620,250]},basketball:{self:[690,365],peer:[1150,345]},lobby:{self:[526,393],peer:[705,393]}};
    const [x,y]=anchors[scene][who];el.style.left=x+'px';el.style.top=(scene==='lobby'?y-4:y-10)+'px';
    if(scene==='basketball' && who==='self' && message.kind==='text'){el.style.left='760px';el.style.maxWidth='232px';el.style.setProperty('--tail-offset','-70px');}
    el.setAttribute('aria-label',message.who+': '+(message.text||stickers.find(s=>s.id===message.sticker)?.name));
    if(message.kind==='sticker')el.append(imageFor(message.sticker));else el.append(make('span','',message.text));
    $('bubbles').append(el);
    if(!held){bubbleTimers.set(who,setTimeout(()=>{el.classList.add('fading');bubbleTimers.set(who,setTimeout(()=>el.remove(),240));},message.kind==='sticker'?3000:4000));}
  }
  function showToast(text){clearTimeout(toastTimer);$('toast').textContent=text;$('toast').hidden=false;toastTimer=setTimeout(()=>$('toast').hidden=true,1800);}
  function closeQuick(){$('quick-panel').hidden=true;$('quick-toggle').setAttribute('aria-expanded','false');}
  function closePicker(){$('panel-stickers').hidden=true;$('composer-stickers').setAttribute('aria-expanded','false');}
  function openChat(open){$('chat-panel').hidden=!open;$('chat-toggle').setAttribute('aria-expanded',String(open));$('chat-toggle').setAttribute('aria-label',open?'Close room chat':'Open room chat');closeQuick();closePicker();if(open){unread=0;updateUnread();renderHistory();}save();}
  function timestamp(){return new Date().toLocaleTimeString('en-GB',{hour:'2-digit',minute:'2-digit'});}
  function append(message){history.push(message);renderHistory();bubble(message);save();}
  function send(kind,value){
    const now=Date.now(); if(now-lastSend<1200){showToast('One moment…');return false;}
    if(kind==='text' && !value.trim())return false;
    lastSend=now;const m={who:'Ben',kind,time:timestamp(),scene};if(kind==='text')m.text=value.trim().slice(0,120);else m.sticker=value;
    append(m);closePicker();closeQuick();$('preview-status').textContent=scene==='lobby'?'Sent to this room · saved in shared history':'Sent to this room · '+(kind==='sticker'?'sticker fades after 3 seconds':'text fades after 4 seconds');
    $('send-status').textContent='Sent to room';clearTimeout(sendTimer);sendTimer=setTimeout(()=>$('send-status').textContent='Enter to send · Esc to close',1800);
    if($('chat-panel').hidden)showToast('Sent to room');return true;
  }
  function stickerButton(sticker, gallery=false){
    const button=make('button',gallery?'sticker-card':'sticker-choice');button.type='button';button.setAttribute('aria-label','Send '+sticker.name+' sticker');button.dataset.sticker=sticker.id;button.append(imageFor(sticker.id),make(gallery?'strong':'span','',sticker.name));if(gallery)button.append(make('small','',sticker.meaning));button.addEventListener('click',()=>send('sticker',sticker.id));return button;
  }
  stickers.forEach(s=>{$('sticker-gallery').append(stickerButton(s,true));$('quick-stickers').append(stickerButton(s));$('panel-stickers').append(stickerButton(s));});
  function setScene(next,log=true){if(!names[next])return;const changed=next!==scene;scene=next;clearBubbles();$('world').dataset.scene=scene;$('scene-image').src='reference/'+scene+'.png';$('scene-image').alt='Actual WhatTheFish '+names[scene]+' capture with proposed chat overlays';document.querySelectorAll('[data-scene]').forEach(b=>{if(b.tagName==='BUTTON')b.setAttribute('aria-pressed',String(b.dataset.scene===scene));});closeQuick();closePicker();if(log&&changed){history.push({kind:'context',text:scene==='lobby'?'Returned to the same room lobby':'Travelled together → '+names[scene]});renderHistory();$('preview-status').textContent='Same room history · '+names[scene]+' preview';}save();}
  document.querySelectorAll('button[data-scene]').forEach(b=>b.addEventListener('click',()=>setScene(b.dataset.scene)));
  $('chat-toggle').addEventListener('click',()=>openChat($('chat-panel').hidden));$('chat-close').addEventListener('click',()=>{openChat(false);$('chat-toggle').focus();});
  $('quick-toggle').addEventListener('click',()=>{const open=$('quick-panel').hidden;openChat(false);$('quick-panel').hidden=!open;$('quick-toggle').setAttribute('aria-expanded',String(open));});
  $('quick-close').addEventListener('click',()=>{closeQuick();$('quick-toggle').focus();});
  $('composer-stickers').addEventListener('click',()=>{const open=$('panel-stickers').hidden;$('panel-stickers').hidden=!open;$('composer-stickers').setAttribute('aria-expanded',String(open));});
  $('composer').addEventListener('submit',e=>{e.preventDefault();if(send('text',$('message').value)){$('message').value='';$('counter').textContent='0 / 120';$('message').blur();}});
  $('message').addEventListener('input',()=>$('counter').textContent=$('message').value.length+' / 120');
  $('incoming').addEventListener('click',()=>{const m={who:scene==='lobby'?'Guest':'BOT 3',kind:'text',text:scene==='lobby'?"Let's play together!":'That was a great pass!',time:timestamp(),scene};append(m);if($('chat-panel').hidden){unread++;updateUnread();save();}$('preview-status').textContent='Simulated incoming message · also saved to room history';});
  function examples(held){clearBubbles();bubble({who:'Ben',kind:'text',text:scene==='lobby'?'Ready when you are!':"Nice pass! I've got you."},held);bubble({who:'BOT 3',kind:'sticker',sticker:'ggwp'},held);}
  $('replay').addEventListener('click',()=>{if(scene==='lobby'){openChat(true);$('preview-status').textContent='Lobby messages appear in the shared room history';return;}if(quiet){showToast('Enable bubbles to preview');return;}examples(false);$('preview-status').textContent='Playing · stickers 3 seconds / text 4 seconds';});
  $('quiet').checked=quiet;$('quiet').addEventListener('change',()=>{quiet=$('quiet').checked;clearBubbles();$('preview-status').textContent=quiet?'Bubbles hidden · all messages still appear in room chat':'Bubbles enabled · replay or send a message';save();});
  document.addEventListener('keydown',e=>{if(e.key==='Escape'){openChat(false);$('message').blur();$('chat-toggle').focus();}});
  new ResizeObserver(entries=>{$('world').style.transform='scale('+entries[0].contentRect.width/1600+')';}).observe($('viewport'));
  setScene(params.get('scene')||scene,false);renderHistory();updateUnread();if(params.get('panel')==='open')openChat(true);if(!quiet)examples(true);
})();
