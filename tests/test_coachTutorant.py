import pytest
import requests
import os
import api_info as a

#def test_post_coachTutorant():
def test_get_coachTutorants_coach_byId():
  r = requests.get(a.api_link() + "coachTutorant/coach/" + a.coachId())
  assert r.status_code == 200, r.status_code
def test_delete_coachTutorant_coach_byId():
  r = requests.delete(a.api_link() + "coachTutorant/coach/" + a.coachId())
  assert r.status_code == 204, r.status_code
def test_get_coachTutorants_tutorant_byId():
  r = requests.get(a.api_link() + "coachTutorant/tutorant/" + a.tutorantId())
  assert r.status_code == 200, r.status_code
def test_put_coachTutorant_tutorant_byId():
  payload = "{\n \"studentIDCoach\":\""+ a.coachId +"\",\n\"studentIDTutorant\":\""+ a.tutorantId +"\",\n\"status\":\"Completed\"\n}"
  headers = {
    'Content-Type': "application/json",
    'cache-control': "no-cache"}
  r = requests.put(a.api_link() + "coachTutorant/tutorant/" + a.tutorantId(), data=payload, headers=headers)
  assert r.status_code == 204, r.status_code
def test_delete_coachTutorant_tutorant_byId():
  r = requests.delete(a.api_link() + "coachTutorant/tutorant/" + a.tutorantId())
  assert r.status_code == 204, r.status_code