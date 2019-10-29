import pytest
import requests
import os
import api_info as a

def test_get_tutorants():
  r = requests.get(a.api_link() + "profile/tutorant")
  assert r.status_code == 200, r.status_code
#def test_post_tutorant():
def test_get_tutorant_byId():
  r = requests.get(a.api_link() + "profile/tutorant/" + a.tutorantId())
  assert r.status_code == 200, r.status_code
def test_delete_tutorant_byId():
  r = requests.delete(a.api_link() + "profile/tutorant/" + a.tutorantId())
  assert r.status_code == 204, r.status_code