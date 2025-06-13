property myHsDown,myHsUp,myGetStartData,myCheckStartData,myStop,myStartLines,myStartLevel,mySendStartData
property myOverScreenText
on beginsprite me
  member("PlayerName").text = string(WebName)
  myHsDown = script("score_get").new()
  myHsDown.SetShowType(1)
  myHsUp = script("score_save").new()
  myGetStartData = script("StartData_get").new(me)
 -- myStop = true
  myCheckStartData = true
  
  mySendStartData = script("StartData_save").new()
   cursor 200
--myHsUp.postScore(member("PlayerName").text, 10000)
end



on DataLoaded me,data,obj
  put data
  if data = "" then
    go to 2
  else
    myStartLevel = value(data.line[1])
    myStartLines = value(data.line[2])
    myStop = false
  end if
  
end


on SendData me,_type,data
  if voidp(data) then exit
  if _type = "StartLevel" then myStartLevel = data
  if _type = "StartLines" then myStartLines = data
  mySendStartData.post(myStartLevel,myStartLines)
end




on exitframe me
  if myCheckStartData=true then
    if myStop = true then
      go to the frame
    else
      go to "Game"
    end if
  end if
end



on GameFinished me,_score
  me.refeshHighScores()
  myHsDown.SetShowType(2)
  -- check if the score is higher
  lowest = myHsDown.GetLowestPersonalScore()
  if _score>lowest then
    myHsUp.postScore(member("PlayerName").text, _score)
  end if
end

on ReturnFromSaveScore me,data
  put data
  if data contains "Highscore" then -- new highscore
    me.NewText(data)
    me.refeshHighScores()
  end if
end



on PersonalHighscores me
  myHsDown.SetShowType(2)
  myHsDown.OutputScores()
end


on ShowGeneralScores me
  myHsDown.SetShowType(1)
  myHsDown.OutputScores()
end


on refeshHighScores me
  myHsDown.downloadScores()
end



on NewText me,_text
  if voidp(myOverScreenText) then myOverScreenText =[]
  myOverScreenText.append(script("OverScreenText").new(130,_text,me))
end


on TextFinished me,obj
  _pos = myOverScreenText.getpos(obj)
  myOverScreenText[_pos].destroy()
  myOverScreenText.deleteone (obj)
end
on destroyoverscreenTxt me
  if voidp(myOverScreenText) then exit
  repeat with i=1 to myOverScreenText.count
    myOverScreenText[i].destroy()
    myOverScreenText[i]=0
  end repeat
  myOverScreenText =[]
end


on GetCounterStartData me,_type
  if _type = "StartLevel" then return myStartLevel
  if _type = "StartLines" then return myStartLines
  return 0
end



on endsprite me
  myHsDown.destroy()
  me.destroyoverscreenTxt()
end
