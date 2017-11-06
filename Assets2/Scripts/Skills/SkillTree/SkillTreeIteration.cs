using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillTreeIteration {

	Skill skills;
	SkillTree skillTree;


	public SkillTreeIteration(SkillTree skillTree){
		this.skillTree = skillTree;
	}

	void parseParents(){
		foreach(Skill skill in skills.Potrait){

			//this.skillTree

			if(hasChildren(skill)){
				parseChildren(skill.children, skill);
			}
		}
	}

	bool hasChildren(Skill skill){
		if(skill.children.Length > 0){
			return true;
		}
		return false;
	}

	void parseChildren(Skill[] children, Skill parent){

	}

}
